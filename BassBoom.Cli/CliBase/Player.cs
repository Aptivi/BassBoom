﻿
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Basolia;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Format.Cache;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terminaux.Base;
using Terminaux.Colors;
using Terminaux.Reader.Inputs;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;

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
        internal static bool rerender = true;
        internal static int currentSong = 1;
        internal static double volume = 1.0;
        internal static bool exiting = false;
        internal static int position = 0;
        internal static bool advance = false;
        internal static bool populate = true;
        internal static bool paused = false;
        internal static string cachedLyric = "";
        internal static readonly List<string> musicFiles = new();
        internal static readonly List<CachedSongInfo> cachedInfos = new();

        public static void PlayerLoop()
        {
            InitBasolia.Init();
            volume = PlaybackTools.GetVolume().baseLinear;

            // First, clear the screen to draw our TUI
            while (!exiting)
            {
                Thread.Sleep(1);
                try
                {
                    // Redraw if necessary
                    bool wasRerendered = rerender;
                    if (rerender)
                    {
                        rerender = false;
                        HandleDraw();
                    }

                    // Current duration
                    position = FileTools.IsOpened ? PlaybackPositioningTools.GetCurrentDuration() : 0;
                    var posSpan = FileTools.IsOpened ? PlaybackPositioningTools.GetCurrentDurationSpan() : new();
                    ProgressBarColor.WriteProgress(100 * (position / (double)total), 2, ConsoleWrappers.ActionWindowHeight() - 8, 6);
                    TextWriterWhereColor.WriteWhere($"{posSpan} / {totalSpan}", 3, ConsoleWrappers.ActionWindowHeight() - 9);
                    TextWriterWhereColor.WriteWhere($"Seek: {PlayerControls.seekRate:0.00} | Vol: {volume:0.00}", ConsoleWrappers.ActionWindowWidth() - $"Seek: {PlayerControls.seekRate:0.00} | Vol: {volume:0.00}".Length - 3, ConsoleWrappers.ActionWindowHeight() - 9);

                    // Check the mode
                    if (PlaybackTools.Playing)
                    {
                        // Print the lyrics, if any
                        if (lyricInstance is not null)
                        {
                            string current = lyricInstance.GetLastLineCurrent();
                            if (current != cachedLyric || wasRerendered)
                            {
                                cachedLyric = current;
                                TextWriterWhereColor.WriteWhere(ConsoleExtensions.GetClearLineToRightSequence(), 0, ConsoleWrappers.ActionWindowHeight() - 10);
                                CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 10, lyricInstance.GetLastLineCurrent());
                            }
                        }
                        else
                            cachedLyric = "";

                        // Wait for any keystroke asynchronously
                        if (ConsoleWrappers.ActionKeyAvailable())
                        {
                            var keystroke = Input.DetectKeypress();
                            HandleKeypressPlayMode(keystroke);
                        }
                    }
                    else
                    {
                        TextWriterWhereColor.WriteWhere(ConsoleExtensions.GetClearLineToRightSequence(), 0, ConsoleWrappers.ActionWindowHeight() - 10);
                        cachedLyric = "";

                        // Wait for any keystroke
                        if (ConsoleWrappers.ActionKeyAvailable())
                        {
                            var keystroke = Input.DetectKeypress();
                            HandleKeypressIdleMode(keystroke);
                        }
                    }
                }
                catch (BasoliaException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia when trying to process the music file.\n\n" + bex.Message);
                    rerender = true;
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia output when trying to process the music file.\n\n" + bex.Message);
                    rerender = true;
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.Playing)
                        PlaybackTools.Stop();
                    InfoBoxColor.WriteInfoBox("There's an unknown error when trying to process the music file.\n\n" + ex.Message);
                    rerender = true;
                }
            }

            // Close the file if open
            if (FileTools.IsOpened)
                FileTools.CloseFile();

            // Restore state
            ConsoleWrappers.ActionCursorVisible(true);
            ColorTools.LoadBack();
        }

        private static void HandleKeypressIdleMode(ConsoleKeyInfo keystroke)
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
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    break;
                case ConsoleKey.A:
                    PlayerControls.PromptForAddSong();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptForAddDirectory();
                    break;
                case ConsoleKey.R:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.RemoveAllSongs();
                    else
                        PlayerControls.RemoveCurrentSong();
                    break;
                case ConsoleKey.Q:
                    PlayerControls.Exit();
                    break;
            }
        }

        private static void HandleKeypressPlayMode(ConsoleKeyInfo keystroke)
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
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptSeek();
                    break;
                case ConsoleKey.Q:
                    PlayerControls.Exit();
                    break;
            }
        }

        private static void HandlePlay()
        {
            foreach (var musicFile in musicFiles.Skip(currentSong - 1))
            {
                if (!advance || exiting)
                    return;
                else
                    populate = true;
                currentSong = musicFiles.IndexOf(musicFile) + 1;
                PlayerControls.PopulateMusicFileInfo(musicFile);
                PlayerControls.RenderSongName(musicFile);
                if (paused)
                {
                    paused = false;
                    PlaybackPositioningTools.SeekToFrame(position);
                }
                PlaybackTools.Play();
                lyricInstance = null;
                rerender = true;
            }
        }

        private static void HandleDraw()
        {
            // Prepare things
            ConsoleWrappers.ActionCursorVisible(false);
            ColorTools.LoadBack();

            // First, print the keystrokes
            string keystrokes = "[SPACE] Play/Pause - [ESC] Stop - [Q] Exit - [H] Help";
            CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 2, keystrokes);

            // Print the separator and the music file info
            string separator = new('=', ConsoleWrappers.ActionWindowWidth());
            CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 4, separator);

            // In case we have no songs in the playlist...
            if (!musicFiles.Any())
                return;

            // Populate music file info, as necessary
            if (populate)
                PlayerControls.PopulateMusicFileInfo(musicFiles[currentSong - 1]);
            PlayerControls.RenderSongName(musicFiles[currentSong - 1]);

            // Now, print the list of songs.
            int startPos = 3;
            int endPos = ConsoleWrappers.ActionWindowHeight() - 10;
            int songsPerPage = endPos - startPos;
            int pages = musicFiles.Count / songsPerPage;
            if (musicFiles.Count % songsPerPage == 0)
                pages--;
            int currentPage = (currentSong - 1) / songsPerPage;
            int startIndex = songsPerPage * currentPage;
            for (int i = 0; i <= songsPerPage - 1; i++)
            {
                // Populate the first pane
                string finalEntry = "";
                int finalIndex = i + startIndex;
                if (finalIndex <= musicFiles.Count - 1)
                {
                    // Here, it's getting uglier as we don't have ElementAt() in IEnumerable, too!
                    var (musicName, musicArtist, musicGenre) = PlayerControls.GetMusicNameArtistGenre(finalIndex);
                    string duration = cachedInfos[finalIndex].DurationSpan;
                    string renderedDuration = $"[{duration}]";
                    string dataObject = $"  {musicArtist} - {musicName}".Truncate(ConsoleWrappers.ActionWindowWidth() - renderedDuration.Length - 5);
                    string spaces = new(' ', ConsoleWrappers.ActionWindowWidth() - 4 - duration.Length - dataObject.Length);
                    finalEntry = dataObject + spaces + renderedDuration;
                }

                // Render an entry
                var finalForeColor = finalIndex == currentSong - 1 ? new Color(ConsoleColors.Green) : new Color(ConsoleColors.Gray);
                int top = startPos + finalIndex - startIndex;
                TextWriterWhereColor.WriteWhereColor(finalEntry + new string(' ', ConsoleWrappers.ActionWindowWidth() - finalEntry.Length), 0, top, finalForeColor);
            }
        }

        private static string Truncate(this string target, int threshold)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            // Try to truncate string. If the string length is bigger than the threshold, it'll be truncated to the length of
            // the threshold, putting three dots next to it. We don't use ellipsis marks here because we're dealing with the
            // terminal, and some terminals and some monospace fonts may not support that character, so we mimick it by putting
            // the three dots.
            if (target.Length > threshold)
                return target[..(threshold - 1)] + "...";
            else
                return target;
        }
    }
}
