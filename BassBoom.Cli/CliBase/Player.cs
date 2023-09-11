
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
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using System;
using System.IO;
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

        public static void PlayerLoop(string musicPath)
        {
            bool exiting = false;
            bool rerender = true;

            // Try to open the file after loading the library
            InfoBoxColor.WriteInfoBox("Loading BassBoom to open {0}...", false, vars: musicPath);
            InitBasolia.Init();
            FileTools.OpenFile(musicPath);
            int total = AudioInfoTools.GetDuration(true);
            var totalSpan = AudioInfoTools.GetDurationSpanFromSamples(total);
            int bufferSize = AudioInfoTools.GetBufferSize();
            double volume = PlaybackTools.GetVolume().baseLinear;
            var format = FormatTools.GetFormatInfo();
            AudioInfoTools.GetId3Metadata(out var managedV1, out var managedV2);

            // Try to open the lyrics
            string lyricsPath = Path.GetDirectoryName(musicPath) + "/" + Path.GetFileNameWithoutExtension(musicPath) + ".lrc";
            Lyric lyricInstance = null;
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
            Console.Title = $"BassBoom CLI - Prototype 6: Lyrics and Information - {musicArtist} - {musicName} [{musicGenre}]";

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

                    // Print the separator
                    string separator = new('=', ConsoleWrappers.ActionWindowWidth());
                    CenteredTextColor.WriteCentered(ConsoleWrappers.ActionWindowHeight() - 4, separator);

                    // Print the music name
                    CenteredTextColor.WriteCentered(1, $"{musicArtist} - {musicName} [{musicGenre}]");

                    // Check the mode
                    if (PlaybackTools.Playing)
                    {
                        // Print the progress bar and the current duration
                        int position = PlaybackPositioningTools.GetCurrentDuration();
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
                                    position += (int)format.rate * 3;
                                    if (position > total)
                                        position = total;
                                    PlaybackPositioningTools.SeekToFrame(position);
                                    break;
                                case ConsoleKey.LeftArrow:
                                    position -= (int)format.rate * 3;
                                    if (position < 0)
                                        position = 0;
                                    PlaybackPositioningTools.SeekToFrame(position);
                                    break;
                                case ConsoleKey.Spacebar:
                                    PlaybackTools.Pause();
                                    break;
                                case ConsoleKey.Escape:
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
                        playerThread = new(HandlePlay);
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

        private static void HandlePlay() =>
            PlaybackTools.Play();
    }
}
