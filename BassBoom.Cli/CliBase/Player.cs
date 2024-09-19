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

using BassBoom.Basolia.File;
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
using Terminaux.Inputs.Styles.Selection;
using Terminaux.Inputs;
using BassBoom.Basolia.Exceptions;
using Terminaux.Inputs.Styles;
using Terminaux.Writer.MiscWriters.Tools;
using Terminaux.Writer.MiscWriters;
using Terminaux.Base.Extensions;

namespace BassBoom.Cli.CliBase
{
    internal static class Player
    {
        internal static Thread? playerThread;
        internal static int position = 0;
        internal static readonly List<string> passedMusicPaths = [];
        internal static readonly Keybinding[] showBindings =
        [
            new("Play/Pause", ConsoleKey.Spacebar),
            new("Stop", ConsoleKey.Escape),
            new("Exit", ConsoleKey.Q),
            new("Help", ConsoleKey.H),
        ];
        internal static readonly Keybinding[] allBindings =
        [
            new("Play/Pause", ConsoleKey.Spacebar),
            new("Stop", ConsoleKey.Escape),
            new("Exit", ConsoleKey.Q),
            new("Increase volume", ConsoleKey.UpArrow),
            new("Decrease volume", ConsoleKey.DownArrow),
            new("Seek backwards", ConsoleKey.LeftArrow),
            new("Seek forwards", ConsoleKey.RightArrow),
            new("Decrease seek duration", ConsoleKey.LeftArrow, ConsoleModifiers.Control),
            new("Increase seek duration", ConsoleKey.RightArrow, ConsoleModifiers.Control),
            new("Song information", ConsoleKey.I),
            new("Add a music file", ConsoleKey.A),
            new("Add a music group from playlist", ConsoleKey.A, ConsoleModifiers.Shift),
            new("Add a music directory to the list (when idle)", ConsoleKey.S),
            new("Previous song", ConsoleKey.B),
            new("Next song", ConsoleKey.N),
            new("Remove current song", ConsoleKey.R),
            new("Remove all songs", ConsoleKey.R, ConsoleModifiers.Control),
            new("Selectively seek (when playing)", ConsoleKey.S),
            new("Seek to previous lyric (when playing)", ConsoleKey.F),
            new("Seek to next lyric (when playing)", ConsoleKey.G),
            new("Seek to current lyric (when playing)", ConsoleKey.J),
            new("Seek to which lyric (when playing)", ConsoleKey.K),
            new("Set repeat checkpoint", ConsoleKey.C),
            new("Seek to repeat checkpoint", ConsoleKey.C, ConsoleModifiers.Shift),
            new("Disco Mode!", ConsoleKey.L),
            new("Enable volume boost", ConsoleKey.V),
            new("Open the equalizer", ConsoleKey.E),
            new("Device and driver information", ConsoleKey.D),
            new("Set device and driver", ConsoleKey.D, ConsoleModifiers.Control),
            new("Reset device and driver", ConsoleKey.D, ConsoleModifiers.Shift),
            new("System information", ConsoleKey.Z),
        ];

        public static void PlayerLoop()
        {
            Common.volume = PlaybackTools.GetVolume(BassBoomCli.basolia).baseLinear;

            // Populate the screen
            Screen playerScreen = new();
            ScreenTools.SetCurrent(playerScreen);

            // Make a screen part to draw our TUI
            ScreenPart screenPart = new();

            // Handle drawing
            screenPart.AddDynamicText(HandleDraw);

            // Current duration
            int hue = 0;
            screenPart.AddDynamicText(() =>
            {
                if (Common.CurrentCachedInfo is null)
                    return "";
                var buffer = new StringBuilder();
                string name = PlayerControls.RenderSongName(Common.CurrentCachedInfo.MusicPath);
                int startPos = 4;
                int endPos = ConsoleWrapper.WindowHeight - 5;
                int songsPerPage = endPos - startPos;
                position = FileTools.IsOpened(BassBoomCli.basolia) ? PlaybackPositioningTools.GetCurrentDuration(BassBoomCli.basolia) : 0;
                var posSpan = FileTools.IsOpened(BassBoomCli.basolia) ? PlaybackPositioningTools.GetCurrentDurationSpan(BassBoomCli.basolia) : new();
                var disco = PlaybackTools.IsPlaying(BassBoomCli.basolia) && Common.enableDisco ? new Color($"hsl:{hue};50;50") : BassBoomCli.white;
                if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                {
                    hue++;
                    if (hue >= 360)
                        hue = 0;
                }
                string boostIndicator = Common.volBoost ? new Color(ConsoleColors.Red).VTSequenceForeground : "";
                string indicator =
                    $"┤ Seek: {PlayerControls.seekRate:0.00} | " +
                    $"{boostIndicator}Volume: {Common.volume * 100:0}%{disco.VTSequenceForeground} ├";
                string lyric = Common.CurrentCachedInfo.LyricInstance is not null ? Common.CurrentCachedInfo.LyricInstance.GetLastLineCurrent(BassBoomCli.basolia) : "";
                string finalLyric = string.IsNullOrWhiteSpace(lyric) ? "..." : lyric;
                buffer.Append(
                    BoxFrameColor.RenderBoxFrame(name, 2, 1, ConsoleWrapper.WindowWidth - 6, songsPerPage, disco) +
                    ProgressBarColor.RenderProgress(100 * (position / (double)Common.CurrentCachedInfo.Duration), 2, ConsoleWrapper.WindowHeight - 5, ConsoleWrapper.WindowWidth - 6, disco, disco) +
                    TextWriterWhereColor.RenderWhereColor($"┤ {posSpan} / {Common.CurrentCachedInfo.DurationSpan} ├", 4, ConsoleWrapper.WindowHeight - 5, disco) +
                    TextWriterWhereColor.RenderWhereColor(indicator, ConsoleWrapper.WindowWidth - ConsoleChar.EstimateCellWidth(indicator) - 4, ConsoleWrapper.WindowHeight - 5, disco) +
                    CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 3, Common.CurrentCachedInfo.LyricInstance is not null && PlaybackTools.IsPlaying(BassBoomCli.basolia) ? $"┤ {finalLyric} ├" : "", disco)
                );
                return buffer.ToString();
            });

            // Render the buffer
            playerScreen.AddBufferedPart("BassBoom Player", screenPart);

            // Then, the main loop
            while (!Common.exiting)
            {
                Thread.Sleep(1);
                try
                {
                    if (!playerScreen.CheckBufferedPart("BassBoom Player"))
                        playerScreen.AddBufferedPart("BassBoom Player", screenPart);
                    ScreenTools.Render();

                    // Handle the keystroke
                    if (ConsoleWrapper.KeyAvailable)
                    {
                        var keystroke = Input.ReadKey();
                        if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                            HandleKeypressPlayMode(keystroke, playerScreen);
                        else
                            HandleKeypressIdleMode(keystroke, playerScreen);
                    }
                }
                catch (BasoliaException bex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia when trying to process the music file.\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia output when trying to process the music file.\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxColor.WriteInfoBox("There's an unknown error when trying to process the music file.\n\n" + ex.Message);
                    playerScreen.RequireRefresh();
                }
            }

            // Close the file if open
            if (FileTools.IsOpened(BassBoomCli.basolia))
                FileTools.CloseFile(BassBoomCli.basolia);

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
                case ConsoleKey.Spacebar:
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    break;
                case ConsoleKey.B:
                    PlayerControls.SeekBeginning();
                    PlayerControls.PreviousSong();
                    Common.redraw = true;
                    break;
                case ConsoleKey.N:
                    PlayerControls.SeekBeginning();
                    PlayerControls.NextSong();
                    Common.redraw = true;
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.A:
                    if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        PlayerControls.PromptForAddSongs();
                    else
                        PlayerControls.PromptForAddSong();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptForAddDirectory();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.R:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.RemoveAllSongs();
                    else
                        PlayerControls.RemoveCurrentSong();
                    Common.redraw = true;
                    break;
                case ConsoleKey.C:
                    if (Common.CurrentCachedInfo is null)
                        return;
                    if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        PlayerControls.SeekTo(Common.CurrentCachedInfo.RepeatCheckpoint);
                    else
                        Common.CurrentCachedInfo.RepeatCheckpoint = PlaybackPositioningTools.GetCurrentDurationSpan(BassBoomCli.basolia);
                    break;
                default:
                    Common.HandleKeypressCommon(keystroke, playerScreen, false);
                    break;
            }
        }

        private static void HandleKeypressPlayMode(ConsoleKeyInfo keystroke, Screen playerScreen)
        {
            switch (keystroke.Key)
            {
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
                    Common.redraw = true;
                    break;
                case ConsoleKey.F:
                    PlayerControls.SeekPreviousLyric();
                    break;
                case ConsoleKey.G:
                    PlayerControls.SeekNextLyric();
                    break;
                case ConsoleKey.J:
                    PlayerControls.SeekCurrentLyric();
                    break;
                case ConsoleKey.K:
                    PlayerControls.SeekWhichLyric();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.N:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    PlayerControls.NextSong();
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    Common.redraw = true;
                    break;
                case ConsoleKey.Spacebar:
                    PlayerControls.Pause();
                    Common.redraw = true;
                    break;
                case ConsoleKey.R:
                    PlayerControls.Stop(false);
                    PlayerControls.SeekBeginning();
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                        PlayerControls.RemoveAllSongs();
                    else
                        PlayerControls.RemoveCurrentSong();
                    Common.redraw = true;
                    break;
                case ConsoleKey.Escape:
                    PlayerControls.Stop();
                    break;
                case ConsoleKey.I:
                    PlayerControls.ShowSongInfo();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.S:
                    PlayerControls.PromptSeek();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.D:
                    PlayerControls.Pause();
                    Common.HandleKeypressCommon(keystroke, playerScreen, false);
                    playerThread = new(HandlePlay);
                    PlayerControls.Play();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.C:
                    if (Common.CurrentCachedInfo is null)
                        return;
                    if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        PlayerControls.SeekTo(Common.CurrentCachedInfo.RepeatCheckpoint);
                    else
                        Common.CurrentCachedInfo.RepeatCheckpoint = PlaybackPositioningTools.GetCurrentDurationSpan(BassBoomCli.basolia);
                    break;
                default:
                    Common.HandleKeypressCommon(keystroke, playerScreen, false);
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
                    {
                        Common.redraw = true;
                        Common.populate = true;
                    }
                    Common.currentPos = Common.cachedInfos.IndexOf(musicFile) + 1;
                    PlayerControls.PopulateMusicFileInfo(musicFile.MusicPath);
                    if (Common.paused)
                    {
                        Common.paused = false;
                        PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, position);
                    }
                    PlaybackTools.Play(BassBoomCli.basolia);
                }
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox($"Playback failure: {ex.Message}");
                Common.failedToPlay = true;
            }
        }

        private static string HandleDraw()
        {
            if (!Common.redraw)
                return "";
            Common.redraw = false;

            // Prepare things
            var drawn = new StringBuilder();
            ConsoleWrapper.CursorVisible = false;

            // First, print the keystrokes
            drawn.Append(KeybindingsWriter.RenderKeybindings(showBindings, 0, ConsoleWrapper.WindowHeight - 1));

            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
            {
                if (passedMusicPaths.Count > 0)
                {
                    foreach (string path in passedMusicPaths)
                    {
                        PlayerControls.PopulateMusicFileInfo(path);
                        Common.populate = true;
                    }
                    passedMusicPaths.Clear();
                }
                else
                {
                    int height = (ConsoleWrapper.WindowHeight - 2) / 2;
                    drawn.Append(CenteredTextColor.RenderCentered(height, "Press 'A' to insert a single song to the playlist, or 'S' to insert the whole music library."));
                    return drawn.ToString();
                }
            }

            // Populate music file info, as necessary
            string name = "";
            if (Common.CurrentCachedInfo is not null)
            {
                if (Common.populate)
                    PlayerControls.PopulateMusicFileInfo(Common.CurrentCachedInfo.MusicPath);
                name = PlayerControls.RenderSongName(Common.CurrentCachedInfo.MusicPath);
            }

            // Now, print the list of songs.
            var choices = new List<InputChoiceInfo>();
            int startPos = 4;
            int endPos = ConsoleWrapper.WindowHeight - 5;
            int songsPerPage = endPos - startPos;
            int max = Common.cachedInfos.Select((_, idx) => idx).Max((idx) => $"  {idx + 1}) ".Length);
            for (int i = 0; i < Common.cachedInfos.Count; i++)
            {
                // Populate the first pane
                var (musicName, musicArtist, _) = PlayerControls.GetMusicNameArtistGenre(i);
                string duration = Common.cachedInfos[i].DurationSpan;
                string songPreview = $"[{duration}] {musicArtist} - {musicName}";
                choices.Add(new($"{i + 1}", songPreview));
            }
            drawn.Append(
                BoxFrameColor.RenderBoxFrame(name, 2, 1, ConsoleWrapper.WindowWidth - 6, songsPerPage) +
                SelectionInputTools.RenderSelections([.. choices], 3, 2, Common.currentPos - 1, songsPerPage, ConsoleWrapper.WindowWidth - 6, selectedForegroundColor: new Color(ConsoleColors.Green), foregroundColor: new Color(ConsoleColors.Silver))
            );
            return drawn.ToString();
        }
    }
}
