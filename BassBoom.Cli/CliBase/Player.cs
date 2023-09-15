
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
using System.IO;
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
        private static Thread playerThread;
        private static Lyric lyricInstance = null;
        private static FrameInfo frameInfo = null;
        private static Id3V1Metadata managedV1 = null;
        private static Id3V2Metadata managedV2 = null;
        private static TimeSpan totalSpan = new();
        private static int total = 0;
        private static (long rate, int channels, int encoding) formatInfo = new();
        private static List<string> musicFiles = new();
        private static bool rerender = true;
        private static int currentSong = 1;
        private static double volume = 1.0;
        private static bool exiting = false;
        private static int position = 0;
        private static bool advance = false;
        private static bool populate = true;
        private static bool regen = true;
        private static bool paused = false;
        private static readonly List<CachedSongInfo> cachedInfos = new();

        public static void PlayerLoop(string musicPath)
        {
            InitBasolia.Init();
            volume = PlaybackTools.GetVolume().baseLinear;

            // First, clear the screen to draw our TUI
            while (!exiting)
            {
                try
                {
                    // If we need to render again, do it
                    if (rerender)
                    {
                        rerender = false;
                        ConsoleWrappers.ActionCursorVisible(false);
                        ColorTools.LoadBack();
                    }

                    // First, print the keystrokes
                    string keystrokes = "[SPACE] Play/Pause - [ESC] Stop - [Q] Exit - [H] Help";
                    CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 2, keystrokes);

                    // Print the separator and the music file info
                    string separator = new('=', ConsoleWrappers.ActionWindowWidth());
                    CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 4, separator);
                    if (populate)
                        ShowMusicFileInfo(musicPath);

                    // Now, print the list of songs.
                    int startPos = 3;
                    int endPos = ConsoleWrappers.ActionWindowHeight() - 12;
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
                            string dataObject = musicFiles[startIndex + i];
                            finalEntry = $"  {dataObject}".Truncate(ConsoleWrappers.ActionWindowWidth() - 2);
                        }

                        // Render an entry
                        var finalForeColor = finalIndex == currentSong - 1 ? new Color(ConsoleColors.Green) : new Color(ConsoleColors.Gray);
                        int top = startPos + finalIndex - startIndex;
                        TextWriterWhereColor.WriteWhere(finalEntry + new string(' ', ConsoleWrappers.ActionWindowWidth() - 2 - finalEntry.Length - 1), 0, top, finalForeColor);
                    }

                    // Check the mode
                    if (PlaybackTools.Playing)
                    {
                        // Print the progress bar and the current duration
                        position = PlaybackPositioningTools.GetCurrentDuration();
                        var posSpan = PlaybackPositioningTools.GetCurrentDurationSpan();
                        ProgressBarColor.WriteProgress(100 * (position / (double)total), 2, ConsoleWrappers.ActionWindowHeight() - 8, 6);
                        TextWriterWhereColor.WriteWhere($"{posSpan} / {totalSpan}", 3, ConsoleWrappers.ActionWindowHeight() - 9);
                        TextWriterWhereColor.WriteWhere($"Vol: {volume:0.00}", ConsoleWrappers.ActionWindowWidth() - $"Vol: {volume:0.00}".Length - 3, ConsoleWrappers.ActionWindowHeight() - 9);

                        // Print the lyrics, if any
                        if (lyricInstance is not null)
                            TextWriterWhereColor.WriteWhere(lyricInstance.GetLastLineCurrent() + ConsoleExtensions.GetClearLineToRightSequence(), 3, ConsoleWrappers.ActionWindowHeight() - 10);

                        // Wait for any keystroke asynchronously
                        if (ConsoleWrappers.ActionKeyAvailable())
                        {
                            var keystroke = Input.DetectKeypress().Key;
                            switch (keystroke)
                            {
                                case ConsoleKey.UpArrow:
                                    volume += 0.05;
                                    if (volume > 1)
                                        volume = 1;
                                    PlaybackTools.SetVolume(volume);
                                    break;
                                case ConsoleKey.DownArrow:
                                    volume -= 0.05;
                                    if (volume < 0)
                                        volume = 0;
                                    PlaybackTools.SetVolume(volume);
                                    break;
                                case ConsoleKey.RightArrow:
                                    position += (int)formatInfo.rate * 3;
                                    if (position > total)
                                        position = total;
                                    PlaybackPositioningTools.SeekToFrame(position);
                                    break;
                                case ConsoleKey.LeftArrow:
                                    position -= (int)formatInfo.rate * 3;
                                    if (position < 0)
                                        position = 0;
                                    PlaybackPositioningTools.SeekToFrame(position);
                                    break;
                                case ConsoleKey.Spacebar:
                                    advance = false;
                                    regen = true;
                                    paused = true;
                                    PlaybackTools.Pause();
                                    break;
                                case ConsoleKey.Escape:
                                    advance = false;
                                    regen = true;
                                    paused = false;
                                    currentSong = 1;
                                    PlaybackTools.Stop();
                                    break;
                                case ConsoleKey.H:
                                    InfoBoxColor.WriteInfoBox(
                                        """
                                    Available keystrokes
                                    ====================

                                    [SPACE]     Play/Pause
                                    [ESC]       Stop
                                    [Q]         Exit
                                    [UP/DOWN]   Volume control
                                    [<-/->]     Seek control
                                    [I]         Song info
                                    [A]         Add a music file
                                    """
                                    );
                                    rerender = true;
                                    break;
                                case ConsoleKey.I:
                                    InfoBoxColor.WriteInfoBox(
                                        $$"""
                                    Song info
                                    =========

                                    Artist: {{(!string.IsNullOrEmpty(managedV2.Artist) ? managedV2.Artist : !string.IsNullOrEmpty(managedV1.Artist) ? managedV1.Artist : "Unknown")}}
                                    Title: {{(!string.IsNullOrEmpty(managedV2.Title) ? managedV2.Title : !string.IsNullOrEmpty(managedV1.Title) ? managedV1.Title : "")}}
                                    Album: {{(!string.IsNullOrEmpty(managedV2.Album) ? managedV2.Album : !string.IsNullOrEmpty(managedV1.Album) ? managedV1.Album : "")}}
                                    Genre: {{(!string.IsNullOrEmpty(managedV2.Genre) ? managedV2.Genre : !string.IsNullOrEmpty(managedV1.Genre.ToString()) ? managedV1.Genre.ToString() : "")}}
                                    Comment: {{(!string.IsNullOrEmpty(managedV2.Comment) ? managedV2.Comment : !string.IsNullOrEmpty(managedV1.Comment) ? managedV1.Comment : "")}}
                                    Duration: {{totalSpan}}
                                    Lyrics: {{(lyricInstance is not null ? $"{lyricInstance.Lines.Count} lines" : "No lyrics")}}

                                    Layer info
                                    ==========

                                    Version: {{frameInfo.Version}}
                                    Layer: {{frameInfo.Layer}}
                                    Rate: {{frameInfo.Rate}}
                                    Mode: {{frameInfo.Mode}}
                                    Mode Ext: {{frameInfo.ModeExt}}
                                    Frame Size: {{frameInfo.FrameSize}}
                                    Flags: {{frameInfo.Flags}}
                                    Emphasis: {{frameInfo.Emphasis}}
                                    Bitrate: {{frameInfo.BitRate}}
                                    ABR Rate: {{frameInfo.AbrRate}}
                                    VBR: {{frameInfo.Vbr}}
                                    """
                                    );
                                    rerender = true;
                                    break;
                                case ConsoleKey.Q:
                                    exiting = true;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Wait for any keystroke
                        if (regen)
                        {
                            regen = false;
                            playerThread = new(HandlePlay);
                        }
                        if (ConsoleWrappers.ActionKeyAvailable())
                        {
                            var keystroke = Input.DetectKeypress().Key;
                            switch (keystroke)
                            {
                                case ConsoleKey.UpArrow:
                                    volume += 0.05;
                                    if (volume > 1)
                                        volume = 1;
                                    PlaybackTools.SetVolume(volume);
                                    break;
                                case ConsoleKey.DownArrow:
                                    volume -= 0.05;
                                    if (volume < 0)
                                        volume = 0;
                                    PlaybackTools.SetVolume(volume);
                                    break;
                                case ConsoleKey.Spacebar:
                                    if (PlaybackTools.State == PlaybackState.Stopped)
                                        // There could be a chance that the music has fully stopped without any user interaction.
                                        PlaybackPositioningTools.SeekToTheBeginning();
                                    advance = true;
                                    playerThread.Start();
                                    SpinWait.SpinUntil(() => PlaybackTools.Playing);
                                    break;
                                case ConsoleKey.H:
                                    InfoBoxColor.WriteInfoBox(
                                        """
                                        Available keystrokes
                                        ====================

                                        [SPACE]     Play/Pause
                                        [ESC]       Stop
                                        [Q]         Exit
                                        [UP/DOWN]   Volume control
                                        [<-/->]     Seek control
                                        [I]         Song info
                                        [A]         Add a music file
                                        """
                                    );
                                    rerender = true;
                                    break;
                                case ConsoleKey.I:
                                    InfoBoxColor.WriteInfoBox(
                                        $$"""
                                        Song info
                                        =========

                                        Artist: {{(!string.IsNullOrEmpty(managedV2.Artist) ? managedV2.Artist : !string.IsNullOrEmpty(managedV1.Artist) ? managedV1.Artist : "Unknown")}}
                                        Title: {{(!string.IsNullOrEmpty(managedV2.Title) ? managedV2.Title : !string.IsNullOrEmpty(managedV1.Title) ? managedV1.Title : "")}}
                                        Album: {{(!string.IsNullOrEmpty(managedV2.Album) ? managedV2.Album : !string.IsNullOrEmpty(managedV1.Album) ? managedV1.Album : "")}}
                                        Genre: {{(!string.IsNullOrEmpty(managedV2.Genre) ? managedV2.Genre : !string.IsNullOrEmpty(managedV1.Genre.ToString()) ? managedV1.Genre.ToString() : "")}}
                                        Comment: {{(!string.IsNullOrEmpty(managedV2.Comment) ? managedV2.Comment : !string.IsNullOrEmpty(managedV1.Comment) ? managedV1.Comment : "")}}
                                        Duration: {{totalSpan}}
                                        Lyrics: {{(lyricInstance is not null ? $"{lyricInstance.Lines.Count} lines" : "No lyrics")}}
                                    
                                        Layer info
                                        ==========
                                    
                                        Version: {{frameInfo.Version}}
                                        Layer: {{frameInfo.Layer}}
                                        Rate: {{frameInfo.Rate}}
                                        Mode: {{frameInfo.Mode}}
                                        Mode Ext: {{frameInfo.ModeExt}}
                                        Frame Size: {{frameInfo.FrameSize}}
                                        Flags: {{frameInfo.Flags}}
                                        Emphasis: {{frameInfo.Emphasis}}
                                        Bitrate: {{frameInfo.BitRate}}
                                        ABR Rate: {{frameInfo.AbrRate}}
                                        VBR: {{frameInfo.Vbr}}
                                        """
                                    );
                                    rerender = true;
                                    break;
                                case ConsoleKey.A:
                                    string path = InfoBoxColor.WriteInfoBoxInput("Enter a path to the music file");
                                    populate = true;
                                    ShowMusicFileInfo(path);
                                    rerender = true;
                                    break;
                                case ConsoleKey.Q:
                                    exiting = true;
                                    advance = false;
                                    break;
                            }
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

        private static bool TryOpenMusicFile(string musicPath)
        {
            try
            {
                if (FileTools.IsOpened)
                    FileTools.CloseFile();
                FileTools.OpenFile(musicPath);
                FileTools.CloseFile();
                return true;
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox("Can't open {0}: {1}", true, vars: new[] { musicPath, ex.Message });
            }
            return false;
        }

        private static void ShowMusicFileInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !populate)
                return;
            populate = false;
            if (!TryOpenMusicFile(musicPath))
                return;
            FileTools.OpenFile(musicPath);
            if (cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                var instance = cachedInfos.Single((csi) => csi.MusicPath == musicPath);
                total = instance.Duration;
                formatInfo = instance.FormatInfo;
                totalSpan = AudioInfoTools.GetDurationSpanFromSamples(total, formatInfo);
                frameInfo = instance.FrameInfo;
                managedV1 = instance.MetadataV1;
                managedV2 = instance.MetadataV2;
                lyricInstance = instance.LyricInstance;
                if (!musicFiles.Contains(musicPath))
                    musicFiles.Add(musicPath);
            }
            else
            {
                InfoBoxColor.WriteInfoBox("Loading BassBoom to open {0}...", false, vars: musicPath);
                total = AudioInfoTools.GetDuration(true);
                totalSpan = AudioInfoTools.GetDurationSpanFromSamples(total);
                formatInfo = FormatTools.GetFormatInfo();
                frameInfo = AudioInfoTools.GetFrameInfo();
                AudioInfoTools.GetId3Metadata(out managedV1, out managedV2);

                // Try to open the lyrics
                string lyricsPath = Path.GetDirectoryName(musicPath) + "/" + Path.GetFileNameWithoutExtension(musicPath) + ".lrc";
                try
                {
                    InfoBoxColor.WriteInfoBox("Trying to open lyrics file {0}...", false, vars: lyricsPath);
                    if (File.Exists(lyricsPath))
                        lyricInstance = LyricReader.GetLyrics(lyricsPath);
                }
                catch (Exception ex)
                {
                    InfoBoxColor.WriteInfoBox("Can't open lyrics file {0}... {1}", vars: new[] { lyricsPath, ex.Message });
                }
                var instance = new CachedSongInfo(musicPath, managedV1, managedV2, total, formatInfo, frameInfo, lyricInstance);
                cachedInfos.Add(instance);
            }

            // Render the song name
            string musicName =
                !string.IsNullOrEmpty(managedV2.Title) ? managedV2.Title :
                !string.IsNullOrEmpty(managedV1.Title) ? managedV1.Title :
                Path.GetFileNameWithoutExtension(musicPath);
            string musicArtist =
                !string.IsNullOrEmpty(managedV2.Artist) ? managedV2.Artist :
                !string.IsNullOrEmpty(managedV1.Artist) ? managedV1.Artist :
                "Unknown Artist";
            string musicGenre =
                !string.IsNullOrEmpty(managedV2.Genre) ? managedV2.Genre :
                managedV1.GenreIndex >= 0 ? $"{managedV1.Genre} [{managedV1.GenreIndex}]" :
                "Unknown Genre";

            // Print the music name
            Console.Title = $"BassBoom CLI - Basolia v0.0.1 - Pre-alpha - {musicArtist} - {musicName} [{musicGenre}]";
            CenteredTextColor.WriteCentered(1, $"{musicArtist} - {musicName} [{musicGenre}]");
            if (!musicFiles.Contains(musicPath))
                musicFiles.Add(musicPath);
        }

        private static void HandlePlay()
        {
            foreach (var musicFile in musicFiles.Skip(currentSong - 1))
            {
                populate = true;
                if (!advance)
                    return;
                currentSong = musicFiles.IndexOf(musicFile) + 1;
                ShowMusicFileInfo(musicFile);
                if (paused)
                {
                    paused = false;
                    PlaybackPositioningTools.SeekToFrame(position);
                }
                PlaybackTools.Play();
                lyricInstance = null;
                rerender = true;
            }
            regen = true;
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
