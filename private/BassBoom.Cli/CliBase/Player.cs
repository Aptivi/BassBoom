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
using Terminaux.Writer.CyclicWriters.Renderer.Tools;
using Terminaux.Writer.MiscWriters;
using Terminaux.Base.Extensions;
using Terminaux.Writer.CyclicWriters;
using Terminaux.Writer.CyclicWriters.Renderer;
using Terminaux.Colors.Transformation;
using BassBoom.Cli.Languages;

namespace BassBoom.Cli.CliBase
{
    internal static class Player
    {
        internal static Thread? playerThread;
        internal static int position = 0;
        internal static readonly List<string> passedMusicPaths = [];

        internal static Keybinding[] ShowBindings =>
        [
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_PLAYPAUSE"), ConsoleKey.Spacebar),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_STOP"), ConsoleKey.Escape),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_QUIT"), ConsoleKey.Q),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_HELP"), ConsoleKey.H),
        ];

        internal static Keybinding[] AllBindings =>
        [
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_PLAYPAUSE"), ConsoleKey.Spacebar),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_STOP"), ConsoleKey.Escape),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_QUIT"), ConsoleKey.Q),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLUMEUP"), ConsoleKey.UpArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLUMEDOWN"), ConsoleKey.DownArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_BACKWARDS"), ConsoleKey.LeftArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_FORWARDS"), ConsoleKey.RightArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_DECREASESEEKDURATION"), ConsoleKey.LeftArrow, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_INCREASESEEKDURATION"), ConsoleKey.RightArrow, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SONGINFO"), ConsoleKey.I),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_ADDFILE"), ConsoleKey.A),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_ADDGROUP"), ConsoleKey.A, ConsoleModifiers.Shift),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_ADDDIR"), ConsoleKey.S),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_PREVSONG"), ConsoleKey.B),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_NEXTSONG"), ConsoleKey.N),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_REMOVECURRSONG"), ConsoleKey.R),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_REMOVEALLSONGS"), ConsoleKey.R, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKSELECTIVE"), ConsoleKey.S),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKTOPREVLYRIC"), ConsoleKey.F),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKTONEXTLYRIC"), ConsoleKey.G),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKTOCURRLYRIC"), ConsoleKey.J),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKTOWHICHLYRIC"), ConsoleKey.K),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SETREPEATCHECKPOINT"), ConsoleKey.C),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_SEEKTOREPEATCHECKPOINT"), ConsoleKey.C, ConsoleModifiers.Shift),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DISCO"), ConsoleKey.L),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_VOLBOOST"), ConsoleKey.V),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_SAVETOPLAYLIST"), ConsoleKey.F1),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_KEYBINDING_PLAYTESTSOUND"), ConsoleKey.F2),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_EQUALIZER"), ConsoleKey.E),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERINFO"), ConsoleKey.D),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERSET"), ConsoleKey.D, ConsoleModifiers.Control),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_DEVICEDRIVERRESET"), ConsoleKey.D, ConsoleModifiers.Shift),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_SYSINFO"), ConsoleKey.Z),
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

                // Get the song name
                var buffer = new StringBuilder();
                string name = PlayerControls.RenderSongName(Common.CurrentCachedInfo.MusicPath);

                // Get the positions and the amount of songs per page
                int startPos = 4;
                int endPos = ConsoleWrapper.WindowHeight - 3;
                int songsPerPage = endPos - startPos;

                // Get the position
                position = FileTools.IsOpened(BassBoomCli.basolia) ? PlaybackPositioningTools.GetCurrentDuration(BassBoomCli.basolia) : 0;
                var posSpan = FileTools.IsOpened(BassBoomCli.basolia) ? PlaybackPositioningTools.GetCurrentDurationSpan(BassBoomCli.basolia) : new();

                // Disco effect!
                var disco = PlaybackTools.IsPlaying(BassBoomCli.basolia) && Common.enableDisco ? new Color($"hsl:{hue};50;50") : BassBoomCli.white;
                if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                {
                    hue++;
                    if (hue >= 360)
                        hue = 0;
                }

                // Render the song list box frame and the duration bar
                var listBoxFrame = new BoxFrame()
                {
                    Text = name,
                    Left = 2,
                    Top = 1,
                    InteriorWidth = ConsoleWrapper.WindowWidth - 6,
                    InteriorHeight = songsPerPage,
                    FrameColor = disco,
                    TitleColor = disco,
                };
                var durationBar = new SimpleProgress((int)(100 * (position / (double)Common.CurrentCachedInfo.Duration)), 100)
                {
                    LeftMargin = 2,
                    RightMargin = 2,
                    ShowPercentage = false,
                    ProgressForegroundColor = TransformationTools.GetDarkBackground(disco),
                    ProgressActiveForegroundColor = disco,
                };
                buffer.Append(
                    listBoxFrame.Render() +
                    ContainerTools.RenderRenderable(durationBar, new(2, ConsoleWrapper.WindowHeight - 3))
                );

                // Render the indicator
                string boostIndicator = Common.volBoost ? new Color(ConsoleColors.Red).VTSequenceForeground : "";
                string indicator =
                    "┤ " + LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_SEEKINDICATOR") + $" {PlayerControls.seekRate:0.00} | " +
                    boostIndicator + LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_VOLINDICATOR") + $" {Common.volume * 100:0}%{disco.VTSequenceForeground} ├";

                // Render the lyric
                string lyric = Common.CurrentCachedInfo.LyricInstance is not null ? Common.CurrentCachedInfo.LyricInstance.GetLastLineCurrent(BassBoomCli.basolia) : "";
                string finalLyric = string.IsNullOrWhiteSpace(lyric) ? "..." : lyric;

                // Render the results
                var lyricText = new AlignedText()
                {
                    Top = ConsoleWrapper.WindowHeight - 3,
                    ForegroundColor = disco,
                    Text = Common.CurrentCachedInfo.LyricInstance is not null && PlaybackTools.IsPlaying(BassBoomCli.basolia) ? $"┤ {finalLyric} ├" : ""
                };
                buffer.Append(
                    TextWriterWhereColor.RenderWhereColor($"┤ {posSpan} / {Common.CurrentCachedInfo.DurationSpan} ├", 4, ConsoleWrapper.WindowHeight - 5, disco) +
                    TextWriterWhereColor.RenderWhereColor(indicator, ConsoleWrapper.WindowWidth - ConsoleChar.EstimateCellWidth(indicator) - 4, ConsoleWrapper.WindowHeight - 5, disco) +
                    lyricText.Render()
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
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_BASOLIAERROR") + "\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (BasoliaOutException bex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_BASOLIAOUTERROR") + "\n\n" + bex.Message);
                    playerScreen.RequireRefresh();
                }
                catch (Exception ex)
                {
                    if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
                        PlaybackTools.Stop(BassBoomCli.basolia);
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_ERROR") + "\n\n" + ex.Message);
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
                case ConsoleKey.F2:
                    PlayerControls.PlayTest();
                    Common.redraw = true;
                    playerScreen.RequireRefresh();
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
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_PLAYBACKFAILED") + $" {ex.Message}");
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
            var keybindings = new Keybindings()
            {
                KeybindingList = ShowBindings,
                Left = 0,
                Top = ConsoleWrapper.WindowHeight - 1,
                Width = ConsoleWrapper.WindowWidth - 1,
            };
            drawn.Append(keybindings.Render());

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
                    var message = new AlignedText()
                    {
                        Top = height,
                        Text = LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TIP"),
                        Settings = new()
                        {
                            Alignment = TextAlignment.Middle
                        }
                    };
                    drawn.Append(message.Render());
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

            // Now, populate the input choice information instances that represent songs
            var choices = new List<InputChoiceInfo>();
            int startPos = 4;
            int endPos = ConsoleWrapper.WindowHeight - 3;
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

            // Render the selections inside the box
            var playlistBoxFrame = new BoxFrame()
            {
                Text = name,
                Left = 2,
                Top = 1,
                InteriorWidth = ConsoleWrapper.WindowWidth - 6,
                InteriorHeight = songsPerPage,
            };
            var playlistSelections = new Selection([.. choices])
            {
                Left = 3,
                Top = 2,
                CurrentSelection = Common.currentPos - 1,
                Height = songsPerPage,
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
