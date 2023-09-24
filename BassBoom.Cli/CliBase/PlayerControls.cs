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

using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Format.Cache;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;

namespace BassBoom.Cli.CliBase
{
    internal static class PlayerControls
    {
        internal static void RaiseVolume()
        {
            Player.volume += 0.05;
            if (Player.volume > 1)
                Player.volume = 1;
            PlaybackTools.SetVolume(Player.volume);
        }

        internal static void LowerVolume()
        {
            Player.volume -= 0.05;
            if (Player.volume < 0)
                Player.volume = 0;
            PlaybackTools.SetVolume(Player.volume);
        }

        internal static void SeekForward()
        {
            Player.position += (int)Player.formatInfo.rate * 3;
            if (Player.position > Player.total)
                Player.position = Player.total;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBackward()
        {
            Player.position -= (int)Player.formatInfo.rate * 3;
            if (Player.position < 0)
                Player.position = 0;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBeginning()
        {
            PlaybackPositioningTools.SeekToTheBeginning();
            Player.position = 0;
        }

        internal static void Play()
        {
            if (PlaybackTools.State == PlaybackState.Stopped)
                // There could be a chance that the music has fully stopped without any user interaction.
                PlaybackPositioningTools.SeekToTheBeginning();
            Player.advance = true;
            Player.rerender = true;
            Player.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.Playing);
        }

        internal static void Pause()
        {
            Player.advance = false;
            Player.regen = true;
            Player.paused = true;
            PlaybackTools.Pause();
        }

        internal static void Stop(bool resetCurrentSong = true)
        {
            Player.advance = false;
            Player.regen = true;
            Player.paused = false;
            if (resetCurrentSong)
                Player.currentSong = 1;
            PlaybackTools.Stop();
        }

        internal static void NextSong()
        {
            Player.currentSong++;
            if (Player.currentSong > Player.musicFiles.Count)
                Player.currentSong = 1;
        }

        internal static void PreviousSong()
        {
            Player.currentSong--;
            if (Player.currentSong <= 0)
                Player.currentSong = Player.musicFiles.Count;
        }

        internal static void PromptForAddSong()
        {
            string path = InfoBoxColor.WriteInfoBoxInput("Enter a path to the music file");
            Player.populate = true;
            PopulateMusicFileInfo(path);
            Player.rerender = true;
        }

        internal static void Exit()
        {
            Player.exiting = true;
            Player.advance = false;
        }

        internal static bool TryOpenMusicFile(string musicPath)
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

        internal static void PopulateMusicFileInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !Player.populate)
                return;
            Player.populate = false;
            if (!TryOpenMusicFile(musicPath))
                return;
            FileTools.OpenFile(musicPath);
            if (Player.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                var instance = Player.cachedInfos.Single((csi) => csi.MusicPath == musicPath);
                Player.total = instance.Duration;
                Player.formatInfo = instance.FormatInfo;
                Player.totalSpan = AudioInfoTools.GetDurationSpanFromSamples(Player.total, Player.formatInfo);
                Player.frameInfo = instance.FrameInfo;
                Player.managedV1 = instance.MetadataV1;
                Player.managedV2 = instance.MetadataV2;
                Player.lyricInstance = instance.LyricInstance;
                if (!Player.musicFiles.Contains(musicPath))
                    Player.musicFiles.Add(musicPath);
            }
            else
            {
                InfoBoxColor.WriteInfoBox("Loading BassBoom to open {0}...", false, vars: musicPath);
                Player.total = AudioInfoTools.GetDuration(true);
                Player.totalSpan = AudioInfoTools.GetDurationSpanFromSamples(Player.total);
                Player.formatInfo = FormatTools.GetFormatInfo();
                Player.frameInfo = AudioInfoTools.GetFrameInfo();
                AudioInfoTools.GetId3Metadata(out Player.managedV1, out Player.managedV2);

                // Try to open the lyrics
                OpenLyrics(musicPath);
                var instance = new CachedSongInfo(musicPath, Player.managedV1, Player.managedV2, Player.total, Player.formatInfo, Player.frameInfo, Player.lyricInstance);
                Player.cachedInfos.Add(instance);
            }
            TextWriterWhereColor.WriteWhere(new string(' ', ConsoleWrappers.ActionWindowWidth()), 0, 1);
            if (!Player.musicFiles.Contains(musicPath))
                Player.musicFiles.Add(musicPath);
        }

        internal static void RenderSongName(string musicPath)
        {
            // Render the song name
            string musicName =
                !string.IsNullOrEmpty(Player.managedV2.Title) ? Player.managedV2.Title :
                !string.IsNullOrEmpty(Player.managedV1.Title) ? Player.managedV1.Title :
                Path.GetFileNameWithoutExtension(musicPath);
            string musicArtist =
                !string.IsNullOrEmpty(Player.managedV2.Artist) ? Player.managedV2.Artist :
                !string.IsNullOrEmpty(Player.managedV1.Artist) ? Player.managedV1.Artist :
                "Unknown Artist";
            string musicGenre =
                !string.IsNullOrEmpty(Player.managedV2.Genre) ? Player.managedV2.Genre :
                Player.managedV1.GenreIndex >= 0 ? $"{Player.managedV1.Genre} [{Player.managedV1.GenreIndex}]" :
                "Unknown Genre";

            // Print the music name
            Console.Title = $"BassBoom CLI - Basolia v0.0.1 - Pre-alpha - {musicArtist} - {musicName} [{musicGenre}]";
            CenteredTextColor.WriteCentered(1, $"{musicArtist} - {musicName} [{musicGenre}]");
        }

        internal static void OpenLyrics(string musicPath)
        {
            string lyricsPath = Path.GetDirectoryName(musicPath) + "/" + Path.GetFileNameWithoutExtension(musicPath) + ".lrc";
            try
            {
                InfoBoxColor.WriteInfoBox("Trying to open lyrics file {0}...", false, vars: lyricsPath);
                if (File.Exists(lyricsPath))
                    Player.lyricInstance = LyricReader.GetLyrics(lyricsPath);
                else
                    Player.lyricInstance = null;
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox("Can't open lyrics file {0}... {1}", vars: new[] { lyricsPath, ex.Message });
            }
        }

        internal static void ShowHelp()
        {
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
                [B]         Previous song
                [N]         Next song
                """
            );
            Player.rerender = true;
        }

        internal static void ShowSongInfo()
        {
            var textsBuilder = new StringBuilder();
            foreach (var text in Player.managedV2.Texts)
                textsBuilder.AppendLine($"T - {text.Item1}: {text.Item2}");
            foreach (var text in Player.managedV2.Extras)
                textsBuilder.AppendLine($"E - {text.Item1}: {text.Item2}");
            InfoBoxColor.WriteInfoBox(
                $$"""
                Song info
                =========

                Artist: {{(!string.IsNullOrEmpty(Player.managedV2.Artist) ? Player.managedV2.Artist : !string.IsNullOrEmpty(Player.managedV1.Artist) ? Player.managedV1.Artist : "Unknown")}}
                Title: {{(!string.IsNullOrEmpty(Player.managedV2.Title) ? Player.managedV2.Title : !string.IsNullOrEmpty(Player.managedV1.Title) ? Player.managedV1.Title : "")}}
                Album: {{(!string.IsNullOrEmpty(Player.managedV2.Album) ? Player.managedV2.Album : !string.IsNullOrEmpty(Player.managedV1.Album) ? Player.managedV1.Album : "")}}
                Genre: {{(!string.IsNullOrEmpty(Player.managedV2.Genre) ? Player.managedV2.Genre : !string.IsNullOrEmpty(Player.managedV1.Genre.ToString()) ? Player.managedV1.Genre.ToString() : "")}}
                Comment: {{(!string.IsNullOrEmpty(Player.managedV2.Comment) ? Player.managedV2.Comment : !string.IsNullOrEmpty(Player.managedV1.Comment) ? Player.managedV1.Comment : "")}}
                Duration: {{Player.totalSpan}}
                Lyrics: {{(Player.lyricInstance is not null ? $"{Player.lyricInstance.Lines.Count} lines" : "No lyrics")}}

                Layer info
                ==========

                Version: {{Player.frameInfo.Version}}
                Layer: {{Player.frameInfo.Layer}}
                Rate: {{Player.frameInfo.Rate}}
                Mode: {{Player.frameInfo.Mode}}
                Mode Ext: {{Player.frameInfo.ModeExt}}
                Frame Size: {{Player.frameInfo.FrameSize}}
                Flags: {{Player.frameInfo.Flags}}
                Emphasis: {{Player.frameInfo.Emphasis}}
                Bitrate: {{Player.frameInfo.BitRate}}
                ABR Rate: {{Player.frameInfo.AbrRate}}
                VBR: {{Player.frameInfo.Vbr}}

                Texts and Extras
                ================

                {{textsBuilder}}
                """
            );
            Player.rerender = true;
        }
    }
}