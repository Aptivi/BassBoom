﻿//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
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
using BassBoom.Basolia.Format.Cache;
using BassBoom.Basolia.Lyrics;
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
using Terminaux.Sequences.Builder.Types;
using Terminaux.Base.Extensions;
using Terminaux.Reader;

namespace BassBoom.Cli.CliBase
{
    internal static class Player
    {
        internal static Thread playerThread;
        internal static Lyric lyricInstance = null;
        internal static FrameInfo frameInfo = null;
        internal static Id3V1Metadata managedV1 = null;
        internal static Id3V2Metadata managedV2 = null;
        internal static TimeSpan totalSpan = new();
        internal static int total = 0;
        internal static (long rate, int channels, int encoding) formatInfo = new();
        internal static int currentSong = 1;
        internal static double volume = 1.0;
        internal static bool exiting = false;
        internal static int position = 0;
        internal static bool advance = false;
        internal static bool populate = true;
        internal static bool paused = false;
        internal static bool failedToPlay = false;
        internal static string cachedLyric = "";
        internal static readonly List<string> musicFiles = [];
        internal static readonly List<CachedSongInfo> cachedInfos = [];
        internal static Version mpgVer;
        internal static Version outVer;
        internal static Version synVer;

        public static void PlayerLoop()
        {
            volume = PlaybackTools.GetVolume().baseLinear;
            exiting = false;
            paused = false;
            populate = true;
            advance = false;

            // Initialize versions
            mpgVer = InitBasolia.MpgLibVersion;
            outVer = InitBasolia.OutLibVersion;
            synVer = InitBasolia.SynLibVersion;

            // Populate the screen
            Screen playerScreen = new();
            ScreenTools.SetCurrent(playerScreen);

            // First, make a screen part to draw our TUI
            ScreenPart screenPart = new();

            // Redraw if necessary
            bool wasRerendered = true;
            screenPart.AddDynamicText(HandleDraw);

            // Current duration
            screenPart.AddDynamicText(() =>
            {
                var buffer = new StringBuilder();
                position = FileTools.IsOpened ? PlaybackPositioningTools.GetCurrentDuration() : 0;
                var posSpan = FileTools.IsOpened ? PlaybackPositioningTools.GetCurrentDurationSpan() : new();
                string indicator =
                    $"Seek: {PlayerControls.seekRate:0.00} | " +
                    $"Volume: {volume:0.00}";
                buffer.Append(
                            ProgressBarColor.RenderProgress(100 * (position / (double)total), 2, ConsoleWrapper.WindowHeight - 8, 3, 3, ConsoleColors.Olive, ConsoleColors.Silver, ConsoleColors.Black) +
                            TextWriterWhereColor.RenderWhere($"{posSpan} / {totalSpan}", 3, ConsoleWrapper.WindowHeight - 9, ConsoleColors.White, ConsoleColors.Black) +
                            TextWriterWhereColor.RenderWhere(indicator, ConsoleWrapper.WindowWidth - indicator.Length - 3, ConsoleWrapper.WindowHeight - 9, ConsoleColors.White, ConsoleColors.Black)
                );
                return buffer.ToString();
            });

            // Get the lyrics
            screenPart.AddDynamicText(() =>
            {
                var buffer = new StringBuilder();
                if (PlaybackTools.Playing)
                {
                    // Print the lyrics, if any
                    if (lyricInstance is not null)
                    {
                        string current = lyricInstance.GetLastLineCurrent();
                        if (current != cachedLyric || wasRerendered)
                        {
                            cachedLyric = current;
                            buffer.Append(
                                        TextWriterWhereColor.RenderWhere(ConsoleClearing.GetClearLineToRightSequence(), 0, ConsoleWrapper.WindowHeight - 10, ConsoleColors.White, ConsoleColors.Black) +
                                        CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 10, lyricInstance.GetLastLineCurrent(), ConsoleColors.White, ConsoleColors.Black)
                            );
                        }
                    }
                    else
                        cachedLyric = "";
                }
                else
                {
                    cachedLyric = "";
                    buffer.Append(
                                TextWriterWhereColor.RenderWhere(ConsoleClearing.GetClearLineToRightSequence(), 0, ConsoleWrapper.WindowHeight - 10, ConsoleColors.White, ConsoleColors.Black)
                    );
                }
                return buffer.ToString();
            });

            // Render the buffer
            playerScreen.AddBufferedPart("BassBoom Player", screenPart);
            playerScreen.ResetResize = false;

            // Then, the main loop
            while (!exiting)
            {
                Thread.Sleep(1);
                try
                {
                    if (!playerScreen.CheckBufferedPart("BassBoom Player"))
                        playerScreen.AddBufferedPart("BassBoom Player", screenPart);
                    wasRerendered = ConsoleResizeHandler.WasResized();
                    ScreenTools.Render();

                    // Handle the keystroke
                    if (ConsoleWrapper.KeyAvailable)
                    {
                        var keystroke = TermReader.ReadKey();
                        if (PlaybackTools.Playing)
                            HandleKeypressPlayMode(keystroke, playerScreen);
                        else
                            HandleKeypressIdleMode(keystroke, playerScreen);
                    }
                }
                catch (BasoliaException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia when trying to process the music file.\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia output when trying to process the music file.\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an unknown error when trying to process the music file.\n\n" + ex.Message);
                    playerScreen.RequireRefresh();
                }
            }

            // Close the file if open
            if (FileTools.IsOpened)
                FileTools.CloseFile();

            // Restore state
            ConsoleWrapper.CursorVisible = true;
            ColorTools.LoadBack();
            playerScreen.RemoveBufferedParts();
            ScreenTools.UnsetCurrent(playerScreen);
        }

        private static void HandleKeypressIdleMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.UpArrow:
                    PlayerControls.RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    PlayerControls.LowerVolume();
                    break;
                case ConsoleKey.Spacebar:
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.B:
                    PlayerControls.SeekBeginning();
                    PlayerControls.PreviousSong();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.N:
                    PlayerControls.SeekBeginning();
                    PlayerControls.NextSong();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.H:
                    PlayerControls.ShowHelp();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.A:
                    PlayerControls.PromptForAddSong();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptForAddDirectory();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.R:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.RemoveAllSongs();
                    else
                        PlayerControls.RemoveCurrentSong();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    PlayerControls.ShowSpecs();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Q:
                    PlayerControls.Exit();
                    break;
            }
        }

        private static void HandleKeypressPlayMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.UpArrow:
                    PlayerControls.RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    PlayerControls.LowerVolume();
                    break;
                case ConsoleKey.RightArrow:
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.seekRate += 0.05d;
                    else
                        PlayerControls.SeekForward();
                    break;
                case ConsoleKey.LeftArrow:
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.seekRate -= 0.05d;
                    else
                        PlayerControls.SeekBackward();
                    break;
                case ConsoleKey.B:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    PlayerControls.PreviousSong();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.N:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    PlayerControls.NextSong();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.Spacebar:
                    PlayerControls.Pause();
                    break;
                case ConsoleKey.R:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.RemoveAllSongs();
                    else
                        PlayerControls.RemoveCurrentSong();
                    break;
                case ConsoleKey.Escape:
                    PlayerControls.Stop();
                    break;
                case ConsoleKey.H:
                    PlayerControls.ShowHelp();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptSeek();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.D:
                    PlayerControls.Pause();
                    PlayerControls.ShowDeviceDriver();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    PlayerControls.ShowSpecs();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Q:
                    PlayerControls.Exit();
                    break;
            }
        }

        private static void HandlePlay()
        {
            try
            {
                foreach (var musicFile in musicFiles.Skip(currentSong - 1))
                {
                    if (!advance || exiting)
                        return;
                    else
                        populate = true;
                    currentSong = musicFiles.IndexOf(musicFile) + 1;
                    PlayerControls.PopulateMusicFileInfo(musicFile);
                    TextWriterRaw.WritePlain(PlayerControls.RenderSongName(musicFile), false);
                    if (paused)
                    {
                        paused = false;
                        PlaybackPositioningTools.SeekToFrame(position);
                    }
                    PlaybackTools.Play();
                }
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox($"Playback failure: {ex.Message}");
                failedToPlay = true;
            }
            finally
            {
                lyricInstance = null;
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
            string separator = new('=', ConsoleWrapper.WindowWidth);
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 4, separator));

            // Write powered by...
            drawn.Append(TextWriterWhereColor.RenderWhere($"[ Powered by BassBoom and MPG123 v{mpgVer} ]", 2, ConsoleWrapper.WindowHeight - 4));

            // In case we have no songs in the playlist...
            if (musicFiles.Count == 0)
            {
                int height = (ConsoleWrapper.WindowHeight - 10) / 2;
                drawn.Append(CenteredTextColor.RenderCentered(height, "Press 'A' to insert a single song to the playlist, or 'S' to insert the whole music library."));
                return drawn.ToString();
            }

            // Populate music file info, as necessary
            if (populate)
                PlayerControls.PopulateMusicFileInfo(musicFiles[currentSong - 1]);
            drawn.Append(PlayerControls.RenderSongName(musicFiles[currentSong - 1]));

            // Now, print the list of songs.
            int startPos = 3;
            int endPos = ConsoleWrapper.WindowHeight - 10;
            int songsPerPage = endPos - startPos;
            int currentPage = (currentSong - 1) / songsPerPage;
            int startIndex = songsPerPage * currentPage;
            var playlist = new StringBuilder();
            for (int i = 0; i <= songsPerPage - 1; i++)
            {
                // Populate the first pane
                string finalEntry = "";
                int finalIndex = i + startIndex;
                if (finalIndex <= musicFiles.Count - 1)
                {
                    var (musicName, musicArtist, _) = PlayerControls.GetMusicNameArtistGenre(finalIndex);
                    string duration = cachedInfos[finalIndex].DurationSpan;
                    string renderedDuration = $"[{duration}]";
                    string dataObject = $"  {musicArtist} - {musicName}".Truncate(ConsoleWrapper.WindowWidth - renderedDuration.Length - 5);
                    string spaces = new(' ', ConsoleWrapper.WindowWidth - 4 - duration.Length - dataObject.Length);
                    finalEntry = dataObject + spaces + renderedDuration;
                }

                // Render an entry
                var finalForeColor = finalIndex == currentSong - 1 ? new Color(ConsoleColors.Green) : new Color(ConsoleColors.Silver);
                int top = startPos + finalIndex - startIndex;
                playlist.Append(
                    $"{CsiSequences.GenerateCsiCursorPosition(1, top + 1)}" +
                    $"{finalForeColor.VTSequenceForeground}" +
                    finalEntry +
                    new string(' ', ConsoleWrapper.WindowWidth - finalEntry.Length)
                );
            }
            drawn.Append(playlist);
            return drawn.ToString();
        }
    }
}
