
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
        internal static double seekRate = 3.0d;

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
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            Player.position += (int)(Player.formatInfo.rate * seekRate);
            if (Player.position > Player.total)
                Player.position = Player.total;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBackward()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            Player.position -= (int)(Player.formatInfo.rate * seekRate);
            if (Player.position < 0)
                Player.position = 0;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBeginning()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            PlaybackPositioningTools.SeekToTheBeginning();
            Player.position = 0;
        }

        internal static void Play()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

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
            Player.paused = true;
            PlaybackTools.Pause();
        }

        internal static void Stop(bool resetCurrentSong = true)
        {
            Player.advance = false;
            Player.paused = false;
            if (resetCurrentSong)
                Player.currentSong = 1;
            PlaybackTools.Stop();
        }

        internal static void NextSong()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            Player.currentSong++;
            if (Player.currentSong > Player.musicFiles.Count)
                Player.currentSong = 1;
        }

        internal static void PreviousSong()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            Player.currentSong--;
            if (Player.currentSong <= 0)
                Player.currentSong = Player.musicFiles.Count;
        }

        internal static void PromptForAddSong()
        {
            string path = InfoBoxColor.WriteInfoBoxInput("Enter a path to the music file");
            int currentPos = Player.position;
            Player.populate = true;
            PopulateMusicFileInfo(path);
            Player.populate = true;
            PopulateMusicFileInfo(Player.musicFiles[Player.currentSong - 1]);
            PlaybackPositioningTools.SeekToFrame(currentPos);
            Player.rerender = true;
        }

        internal static void PromptForAddDirectory()
        {
            string path = InfoBoxColor.WriteInfoBoxInput("Enter a path to the music library directory");
            if (Directory.Exists(path))
            {
                int currentPos = Player.position;
                var musicFiles = Directory.GetFiles(path, "*.mp3");
                foreach (string musicFile in musicFiles)
                {
                    Player.populate = true;
                    PopulateMusicFileInfo(musicFile);
                }
                Player.populate = true;
                PopulateMusicFileInfo(Player.musicFiles[Player.currentSong - 1]);
                PlaybackPositioningTools.SeekToFrame(currentPos);
            }
            else
                InfoBoxColor.WriteInfoBox("Music library directory is not found.");
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
                Player.rerender = true;
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
            var (musicName, musicArtist, _) = GetMusicNameArtistGenre(musicPath);

            // Print the music name
            Console.Title = $"BassBoom CLI - Basolia v0.0.2 - Alpha 2 • {musicArtist} - {musicName}";
            CenteredTextColor.WriteCentered(1, $"Now playing: {musicArtist} - {musicName}");
        }

        internal static (string musicName, string musicArtist, string musicGenre) GetMusicNameArtistGenre(string musicPath)
        {
            var metadatav2 = Player.managedV2;
            var metadatav1 = Player.managedV1;
            string musicName =
                !string.IsNullOrEmpty(metadatav2.Title) ? metadatav2.Title :
                !string.IsNullOrEmpty(metadatav1.Title) ? metadatav1.Title :
                Path.GetFileNameWithoutExtension(musicPath);
            string musicArtist =
                !string.IsNullOrEmpty(metadatav2.Artist) ? metadatav2.Artist :
                !string.IsNullOrEmpty(metadatav1.Artist) ? metadatav1.Artist :
                "Unknown Artist";
            string musicGenre =
                !string.IsNullOrEmpty(metadatav2.Genre) ? metadatav2.Genre :
                metadatav1.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                "Unknown Genre";
            return (musicName, musicArtist, musicGenre);
        }

        internal static (string musicName, string musicArtist, string musicGenre) GetMusicNameArtistGenre(int cachedInfoIdx)
        {
            var cachedInfo = Player.cachedInfos[cachedInfoIdx];
            var metadatav2 = cachedInfo.MetadataV2;
            var metadatav1 = cachedInfo.MetadataV1;
            var path = cachedInfo.MusicPath;
            string musicName =
                !string.IsNullOrEmpty(metadatav2.Title) ? metadatav2.Title :
                !string.IsNullOrEmpty(metadatav1.Title) ? metadatav1.Title :
                Path.GetFileNameWithoutExtension(path);
            string musicArtist =
                !string.IsNullOrEmpty(metadatav2.Artist) ? metadatav2.Artist :
                !string.IsNullOrEmpty(metadatav1.Artist) ? metadatav1.Artist :
                "Unknown Artist";
            string musicGenre =
                !string.IsNullOrEmpty(metadatav2.Genre) ? metadatav2.Genre :
                metadatav1.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                "Unknown Genre";
            return (musicName, musicArtist, musicGenre);
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

        internal static void RemoveCurrentSong()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            Player.cachedInfos.RemoveAt(Player.currentSong - 1);
            Player.musicFiles.RemoveAt(Player.currentSong - 1);
            if (Player.musicFiles.Count > 0)
            {
                Player.currentSong--;
                if (Player.currentSong == 0)
                    Player.currentSong = 1;
                Player.populate = true;
                PopulateMusicFileInfo(Player.musicFiles[Player.currentSong - 1]);
            }
            Player.rerender = true;
        }

        internal static void RemoveAllSongs()
        {
            // In case we have no songs in the playlist...
            if (!Player.musicFiles.Any())
                return;

            for (int i = Player.musicFiles.Count; i > 0; i--)
                RemoveCurrentSong();
        }

        internal static void ShowHelp()
        {
            InfoBoxColor.WriteInfoBox(
                """
                Available keystrokes
                ====================

                [SPACE]             Play/Pause
                [ESC]               Stop
                [Q]                 Exit
                [UP/DOWN]           Volume control
                [<-/->]             Seek control
                [CTRL] + [<-/->]    Seek duration control
                [I]                 Song info
                [A]                 Add a music file
                [S]                 Add a music directory to the playlist
                [B]                 Previous song
                [N]                 Next song
                [R]                 Remove current song
                [CTRL] + [R]        Remove all songs
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
