//
// BassBoom  Copyright (C) 2023-2025  Aptivi
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Languages;
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Base.Extensions;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Inputs;
using Terminaux.Inputs.Styles;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.CyclicWriters.Graphical;
using Terminaux.Writer.CyclicWriters.Renderer;
using Terminaux.Writer.CyclicWriters.Renderer.Tools;
using Terminaux.Writer.CyclicWriters.Simple;

namespace BassBoom.Cli.CliBase
{
    internal static class Radio
    {
        internal static Thread? playerThread;

        internal static Keybinding[] AllBindings =>
        [
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_PLAYPAUSE"), ConsoleKey.Spacebar),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_STOP"), ConsoleKey.Escape),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_QUIT"), ConsoleKey.Q),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLUMEUP"), ConsoleKey.UpArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLUMEDOWN"), ConsoleKey.DownArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_STATIONINFO"), ConsoleKey.I),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_STATIONINFOEXT"), ConsoleKey.I, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_ADDSTATION"), ConsoleKey.A),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_ADDSTATIONGROUP"), ConsoleKey.A, ConsoleModifiers.Shift),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_PREVSTATION"), ConsoleKey.B),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_NEXTSTATION"), ConsoleKey.N),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_REMOVECURRSTATION"), ConsoleKey.R),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_KEYBINDING_REMOVEALLSTATIONS"), ConsoleKey.R, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DISCO"), ConsoleKey.L),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLBOOST"), ConsoleKey.V),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_SAVETOPLAYLIST"), ConsoleKey.F1),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_EQUALIZER"), ConsoleKey.E),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERINFO"), ConsoleKey.D),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERSET"), ConsoleKey.D, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERRESET"), ConsoleKey.D, ConsoleModifiers.Shift),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_SYSINFO"), ConsoleKey.Z),
        ];

        public static void RadioLoop()
        {
            Common.volume = PlaybackTools.GetVolume(BassBoomCli.basolia).baseLinear;
            Common.isRadioMode = true;

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
                if (Common.CurrentCachedInfo is null)
                    return "";

                // Get the name
                string name = RadioControls.RenderStationName();

                // Get the positions and the amount of stations per page
                int startPos = 4;
                int endPos = ConsoleWrapper.WindowHeight - 1;
                int stationsPerPage = endPos - startPos;

                // Get the boost indicator
                var buffer = new StringBuilder();
                string boostIndicator = Common.volBoost ? new Color(ConsoleColors.Red).VTSequenceForeground() : "";

                // Disco effect!
                var disco = PlaybackTools.IsPlaying(BassBoomCli.basolia) && Common.enableDisco ? new Color($"hsl:{hue};50;50") : BassBoomCli.white;
                string indicator = $"┤ {boostIndicator}" + LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_VOLINDICATOR") + $" {Common.volume * 100:0}%{disco.VTSequenceForeground()} ├";
                if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                {
                    hue++;
                    if (hue >= 360)
                        hue = 0;
                }

                // Render the station list box frame and the indicator
                var listBoxFrame = new BoxFrame()
                {
                    Text = name,
                    Left = 2,
                    Top = 1,
                    Width = ConsoleWrapper.WindowWidth - 6,
                    Height = stationsPerPage,
                    FrameColor = disco,
                    TitleColor = disco,
                };
                buffer.Append(
                    listBoxFrame.Render() +
                    TextWriterWhereColor.RenderWhereColor(indicator, ConsoleWrapper.WindowWidth - ConsoleChar.EstimateCellWidth(indicator) - 4, ConsoleWrapper.WindowHeight - 3, disco)
                );
                return buffer.ToString();
            });

            // Render the buffer
            radioScreen.AddBufferedPart("BassBoom Player", screenPart);

            // Then, the main loop
            while (!Common.exiting)
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
                        var keystroke = Input.ReadKey();
                        if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                            HandleKeypressPlayMode(keystroke, radioScreen);
                        else
                            HandleKeypressIdleMode(keystroke, radioScreen);
                    }
                }
                catch (BasoliaException bex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_BASOLIAERROR") + "\n\n" + bex.Message);
                    radioScreen.RequireRefresh();
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_BASOLIAOUTERROR") + "\n\n" + bex.Message);
                    radioScreen.RequireRefresh();
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_ERROR") + "\n\n" + ex.Message);
                    radioScreen.RequireRefresh();
                }
            }

            // Close the file if open
            if (FileTools.IsOpened(BassBoomCli.basolia))
                FileTools.CloseFile(BassBoomCli.basolia);

            // Restore state
            ConsoleWrapper.CursorVisible = true;
            ConsoleColoring.LoadBack();
            radioScreen.RemoveBufferedParts();
            ScreenTools.UnsetCurrent(radioScreen);
        }

        private static void HandleKeypressIdleMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.Spacebar:
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.B:
                    RadioControls.PreviousStation();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.N:
                    RadioControls.NextStation();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.I:
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.ShowExtendedStationInfo();
                    else
                        RadioControls.ShowStationInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.A:
                    if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        RadioControls.PromptForAddStations();
                    else
                        RadioControls.PromptForAddStation();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.R:
                    RadioControls.Stop(false);
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.RemoveAllStations();
                    else
                        RadioControls.RemoveCurrentStation();
                    playerScreen.RequireRefresh();
                    break;
                default:
                    Common.HandleKeypressCommon(keystroke, playerScreen, true);
                    break;
            }
        }

        private static void HandleKeypressPlayMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.B:
                    RadioControls.Stop(false);
                    RadioControls.PreviousStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.N:
                    RadioControls.Stop(false);
                    RadioControls.NextStation();
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Spacebar:
                    RadioControls.Pause();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.R:
                    RadioControls.Stop(false);
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.RemoveAllStations();
                    else
                        RadioControls.RemoveCurrentStation();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Escape:
                    RadioControls.Stop();
                    break;
                case ConsoleKey.I:
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        RadioControls.ShowExtendedStationInfo();
                    else
                        RadioControls.ShowStationInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.D:
                    RadioControls.Pause();
                    Common.HandleKeypressCommon(keystroke, playerScreen, true);
                    playerThread = new(HandlePlay);
                    RadioControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                default:
                    Common.HandleKeypressCommon(keystroke, playerScreen, true);
                    break;
            }
        }

        private static void HandlePlay()
        {
            try
            {
                foreach (var musicFile in Common.cachedInfos.Skip(Common.currentPos - 1))
                {
                    if (!Common.advance || Common.exiting)
                        return;
                    else
                        Common.populate = true;
                    Common.currentPos = Common.cachedInfos.IndexOf(musicFile) + 1;
                    RadioControls.PopulateRadioStationInfo(musicFile.MusicPath);
                    if (Common.paused)
                        Common.paused = false;
                    PlaybackTools.Play(BassBoomCli.basolia);
                }
            }
            catch (Exception ex)
            {
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_PLAYBACKFAILED") + $" {ex.Message}");
                Common.failedToPlay = true;
            }
        }

        private static string HandleDraw()
        {
            if (!ScreenTools.CurrentScreen?.RefreshWasDone ?? false)
                return "";

            // Prepare things
            var drawn = new StringBuilder();
            ConsoleWrapper.CursorVisible = false;

            // First, print the keystrokes
            var keybindings = new Keybindings()
            {
                KeybindingList = Player.ShowBindings,
                Width = ConsoleWrapper.WindowWidth - 1,
            };
            drawn.Append(RendererTools.RenderRenderable(keybindings, new(0, ConsoleWrapper.WindowHeight - 1)));

            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
            {
                int height = (ConsoleWrapper.WindowHeight - 2) / 2;
                var message = new AlignedText()
                {
                    Top = height,
                    Text = LanguageTools.GetLocalized("BASSBOOM_APP_RADIO_TIP"),
                    Settings = new()
                    {
                        Alignment = TextAlignment.Middle
                    }
                };
                drawn.Append(message.Render());
                return drawn.ToString();
            }

            // Populate music file info, as necessary
            string name = "";
            if (Common.CurrentCachedInfo is not null)
            {
                if (Common.populate)
                    RadioControls.PopulateRadioStationInfo(Common.CurrentCachedInfo.MusicPath);
                name = RadioControls.RenderStationName();
            }

            // Now, print the list of stations.
            var choices = new List<InputChoiceInfo>();
            int startPos = 4;
            int endPos = ConsoleWrapper.WindowHeight - 1;
            int stationsPerPage = endPos - startPos;
            int max = Common.cachedInfos.Select((_, idx) => idx).Max((idx) => $"  {idx + 1}) ".Length);
            for (int i = 0; i < Common.cachedInfos.Count; i++)
            {
                // Populate the first pane
                string stationName = Common.cachedInfos[i].StationName;
                string stationPreview = $"{stationName}";
                choices.Add(new($"{i + 1}", stationPreview));
            }

            // Print the list of stations.
            var playlistBoxFrame = new BoxFrame()
            {
                Text = name,
                Left = 2,
                Top = 1,
                Width = ConsoleWrapper.WindowWidth - 6,
                Height = stationsPerPage
            };
            var playlistSelections = new Selection([.. choices])
            {
                Left = 3,
                Top = 2,
                CurrentSelection = Common.currentPos - 1,
                Height = stationsPerPage,
                Width = ConsoleWrapper.WindowWidth - 6,
                Settings = new()
                {
                    SelectedOptionColor = ConsoleColors.Green,
                    OptionColor = ConsoleColors.Silver,
                }
            };
            drawn.Append(
                playlistBoxFrame.Render() +
                playlistSelections.Render()
            );
            return drawn.ToString();
        }
    }
}
