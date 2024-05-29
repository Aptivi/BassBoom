//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of BassBoom
//
// BassBoom is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BassBoom is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using BassBoom.Basolia;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;
using Terminaux.Reader;
using Terminaux.Inputs.Styles.Selection;
using Terminaux.Inputs;
using BassBoom.Cli.Tools;

namespace BassBoom.Cli.CliBase
{
    internal static class Radio
    {
        internal static Thread playerThread;
        internal static FrameInfo frameInfo = null;
        internal static (long rate, int channels, int encoding) formatInfo = new();
        internal static int currentStation = 1;
        internal static double volume = 1.0;
        internal static bool exiting = false;
        internal static bool advance = false;
        internal static bool populate = true;
        internal static bool paused = false;
        internal static bool failedToPlay = false;
        internal static bool enableDisco = false;
        internal static readonly List<CachedSongInfo> cachedInfos = [];

        public static void RadioLoop()
        {
            volume = PlaybackTools.GetVolume().baseLinear;
            exiting = false;
            paused = false;
            populate = true;
            advance = false;

            // Populate the screen
            Screen radioScreen = new();
            ScreenTools.SetCurrent(radioScreen);

            // Make a screen part to draw our TUI
            ScreenPart screenPart = new();

            // Handle drawing
            screenPart.AddDynamicText(HandleDraw);

            // Current volume
            int hue = 0;
            screenPart.AddDynamicText(() =>
            {
                var buffer = new StringBuilder();
                string indicator = $"╣ Volume: {volume:0.00} ╠";
                var disco = PlaybackTools.Playing && enableDisco ? new Color($"hsl:{hue};50;50") : BassBoomCli.white;
                if (PlaybackTools.Playing)
                {
                    hue++;
                    if (hue >= 360)
                        hue = 0;
                }
                buffer.Append(
                    BoxFrameColor.RenderBoxFrame(2, ConsoleWrapper.WindowHeight - 8, ConsoleWrapper.WindowWidth - 6, 1, disco) +
                    TextWriterWhereColor.RenderWhereColor(indicator, ConsoleWrapper.WindowWidth - indicator.Length - 4, ConsoleWrapper.WindowHeight - 8, disco)
                );
                return buffer.ToString();
            });

            // Render the buffer
            radioScreen.AddBufferedPart("BassBoom Player", screenPart);
            radioScreen.ResetResize = false;

            // Then, the main loop
            while (!exiting)
            {
                Thread.Sleep(1);
                try
                {
                    if (!radioScreen.CheckBufferedPart("BassBoom Player"))
                        radioScreen.AddBufferedPart("BassBoom Player", screenPart);
                    ScreenTools.Render();

                    // Handle the keystroke
                    if (ConsoleWrapper.KeyAvailable)
                    {
                        var keystroke = TermReader.ReadKey();
                        if (PlaybackTools.Playing)
                            HandleKeypressPlayMode(keystroke, radioScreen);
                        else
                            HandleKeypressIdleMode(keystroke, radioScreen);
                    }
                }
                catch (BasoliaException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia when trying to process the music file.\n\n" + bex.Message);
                    radioScreen.RequireRefresh();
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia output when trying to process the music file.\n\n" + bex.Message);
                    radioScreen.RequireRefresh();
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an unknown error when trying to process the music file.\n\n" + ex.Message);
                    radioScreen.RequireRefresh();
                }
            }

            // Close the file if open
            if (FileTools.IsOpened)
                FileTools.CloseFile();

            // Restore state
            ConsoleWrapper.CursorVisible = true;
            ColorTools.LoadBack();
            radioScreen.RemoveBufferedParts();
            ScreenTools.UnsetCurrent(radioScreen);
        }

        private static void HandleKeypressIdleMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.UpArrow:
                    RadioControls.RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    RadioControls.LowerVolume();
                    break;
                case ConsoleKey.Spacebar:
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    break;
                case ConsoleKey.B:
                    RadioControls.PreviousStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    break;
                case ConsoleKey.N:
                    RadioControls.NextStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    break;
                case ConsoleKey.H:
                    RadioControls.ShowHelp();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.I:
                    RadioControls.ShowStationInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.A:
                    RadioControls.PromptForAddStation();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.R:
                    RadioControls.Stop(false);
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.RemoveAllStations();
                    else
                        RadioControls.RemoveCurrentStation();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    RadioControls.ShowSpecs();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Q:
                    RadioControls.Exit();
                    break;
            }
        }

        private static void HandleKeypressPlayMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.UpArrow:
                    RadioControls.RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    RadioControls.LowerVolume();
                    break;
                case ConsoleKey.B:
                    RadioControls.Stop(false);
                    RadioControls.PreviousStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    break;
                case ConsoleKey.N:
                    RadioControls.Stop(false);
                    RadioControls.NextStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    break;
                case ConsoleKey.Spacebar:
                    RadioControls.Pause();
                    break;
                case ConsoleKey.R:
                    RadioControls.Stop(false);
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.RemoveAllStations();
                    else
                        RadioControls.RemoveCurrentStation();
                    break;
                case ConsoleKey.Escape:
                    RadioControls.Stop();
                    break;
                case ConsoleKey.H:
                    RadioControls.ShowHelp();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.I:
                    RadioControls.ShowStationInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.D:
                    RadioControls.Pause();
                    RadioControls.ShowDeviceDriver();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    RadioControls.ShowSpecs();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.L:
                    enableDisco = !enableDisco;
                    break;
                case ConsoleKey.Q:
                    RadioControls.Exit();
                    break;
            }
        }

        private static void HandlePlay()
        {
            try
            {
                foreach (var musicFile in cachedInfos.Skip(currentStation - 1))
                {
                    if (!advance || exiting)
                        return;
                    else
                        populate = true;
                    currentStation = cachedInfos.IndexOf(musicFile) + 1;
                    RadioControls.PopulateRadioStationInfo(musicFile.MusicPath);
                    TextWriterRaw.WritePlain(RadioControls.RenderStationName(), false);
                    if (paused)
                        paused = false;
                    PlaybackTools.Play();
                }
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox($"Playback failure: {ex.Message}");
                failedToPlay = true;
            }
        }

        private static string HandleDraw()
        {
            // Prepare things
            var drawn = new StringBuilder();
            ConsoleWrapper.CursorVisible = false;

            // First, print the keystrokes
            string keystrokes =
                "[SPACE] Play/Pause" +
                " - [ESC] Stop" +
                " - [Q] Exit" +
                " - [H] Help";
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 2, keystrokes));

            // Print the separator and the music file info
            string separator = new('═', ConsoleWrapper.WindowWidth);
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 4, separator));

            // Write powered by...
            drawn.Append(TextWriterWhereColor.RenderWhere($"╣ Powered by BassBoom and MPG123 v{BassBoomCli.mpgVer} ╠", 2, ConsoleWrapper.WindowHeight - 4));

            // In case we have no stations in the playlist...
            if (cachedInfos.Count == 0)
            {
                int height = (ConsoleWrapper.WindowHeight - 10) / 2;
                drawn.Append(CenteredTextColor.RenderCentered(height, "Press 'A' to insert a radio station to the playlist."));
                return drawn.ToString();
            }

            // Populate music file info, as necessary
            if (populate)
                RadioControls.PopulateRadioStationInfo(cachedInfos[currentStation - 1].MusicPath);
            drawn.Append(RadioControls.RenderStationName());

            // Now, print the list of stations.
            var choices = new List<InputChoiceInfo>();
            int startPos = 4;
            int endPos = ConsoleWrapper.WindowHeight - 10;
            int stationsPerPage = endPos - startPos;
            int max = cachedInfos.Select((_, idx) => idx).Max((idx) => $"  {idx + 1}) ".Length);
            for (int i = 0; i < cachedInfos.Count; i++)
            {
                // Populate the first pane
                string stationName = cachedInfos[i].StationName;
                string duration = cachedInfos[i].DurationSpan;
                string stationPreview = $"[{duration}] {stationName}";
                choices.Add(new($"{i + 1}", stationPreview));
            }
            drawn.Append(
                BoxFrameColor.RenderBoxFrame(2, 3, ConsoleWrapper.WindowWidth - 6, stationsPerPage) +
                SelectionInputTools.RenderSelections([.. choices], 3, 4, currentStation - 1, stationsPerPage, ConsoleWrapper.WindowWidth - 6, selectedForegroundColor: new Color(ConsoleColors.Green), foregroundColor: new Color(ConsoleColors.Silver))
            );
            return drawn.ToString();
        }
    }
}
