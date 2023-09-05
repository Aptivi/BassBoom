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
using BassBoom.Basolia.Playback;
using System;
using System.IO;
using System.Threading;
using Terminaux.Colors;
using Terminaux.Reader.Inputs;
using Terminaux.Reader.Tools;
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
            InitBasolia.Init();
            FileTools.OpenFile(musicPath);
            int total = AudioInfoTools.GetDuration(true);
            int bufferSize = AudioInfoTools.GetBufferSize();
            double volume = PlaybackTools.GetVolume().baseLinear;

            // First, clear the screen to draw our TUI
            while (!exiting)
            {
                try
                {
                    // If we need to render again, do it
                    if (rerender)
                    {
                        rerender = false;
                        ConsoleTools.ActionCursorVisible(false);
                        ColorTools.LoadBack();
                    }

                    // First, print the keystrokes
                    string keystrokes = "[SPACE] Play/Pause - [ESC] Stop - [Q] Exit - [UP/DOWN] Vol - [<-/->] Seek";
                    CenteredTextColor.WriteCentered(ConsoleTools.ActionWindowHeight() - 2, keystrokes);

                    // Print the separator
                    string separator = new('=', ConsoleTools.ActionWindowWidth());
                    CenteredTextColor.WriteCentered(ConsoleTools.ActionWindowHeight() - 4, separator);

                    // Print the music name
                    string musicName = Path.GetFileNameWithoutExtension(musicPath);
                    CenteredTextColor.WriteCentered(1, musicName);

                    // Check the mode
                    if (PlaybackTools.Playing)
                    {
                        // Print the progress bar
                        int position = PlaybackPositioningTools.GetCurrentDuration();
                        ProgressBarColor.WriteProgress(100 * (position / (double)total), 2, ConsoleTools.ActionWindowHeight() - 8, 6);

                        // Wait for any keystroke asynchronously
                        if (ConsoleTools.ActionKeyAvailable())
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
                                    position += bufferSize * 16;
                                    if (position > total)
                                        position = total;
                                    PlaybackPositioningTools.SeekToFrame(position);
                                    break;
                                case ConsoleKey.LeftArrow:
                                    position -= bufferSize * 16;
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
            ConsoleTools.ActionCursorVisible(true);
            ColorTools.LoadBack();
        }

        private static void HandlePlay() =>
            PlaybackTools.Play();
    }
}