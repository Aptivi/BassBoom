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

using BassBoom.Basolia.Enumerations;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base;
using Terminaux.Colors.Data;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;

namespace BassBoom.Cli.CliBase
{
    internal static class PlayerControls
    {
        internal static double seekRate = 3.0d;

        internal static void SeekForward()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Player.position += (int)(Player.formatInfo.rate * seekRate);
            if (Player.position > Player.total)
                Player.position = Player.total;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBackward()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Player.position -= (int)(Player.formatInfo.rate * seekRate);
            if (Player.position < 0)
                Player.position = 0;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBeginning()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackPositioningTools.SeekToTheBeginning();
            Player.position = 0;
        }

        internal static void Play()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            if (PlaybackTools.State == PlaybackState.Stopped)
                // There could be a chance that the music has fully stopped without any user interaction.
                PlaybackPositioningTools.SeekToTheBeginning();
            Common.advance = true;
            Player.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.Playing || Common.failedToPlay);
            Common.failedToPlay = false;
        }

        internal static void Pause()
        {
            Common.advance = false;
            Common.paused = true;
            PlaybackTools.Pause();
        }

        internal static void Stop(bool resetCurrentSong = true)
        {
            Common.advance = false;
            Common.paused = false;
            if (resetCurrentSong)
                Common.currentPos = 1;
            PlaybackTools.Stop();
        }

        internal static void NextSong()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Common.currentPos++;
            if (Common.currentPos > Common.cachedInfos.Count)
                Common.currentPos = 1;
        }

        internal static void PreviousSong()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Common.currentPos--;
            if (Common.currentPos <= 0)
                Common.currentPos = Common.cachedInfos.Count;
        }

        internal static void PromptForAddSong()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the music file");
            if (File.Exists(path))
            {
                int currentPos = Player.position;
                Common.populate = true;
                PopulateMusicFileInfo(path);
                Common.populate = true;
                PopulateMusicFileInfo(Common.cachedInfos[Common.currentPos - 1].MusicPath);
                PlaybackPositioningTools.SeekToFrame(currentPos);
            }
            else
                InfoBoxColor.WriteInfoBox($"File \"{path}\" doesn't exist.");
        }

        internal static void PromptForAddDirectory()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the music library directory");
            if (Directory.Exists(path))
            {
                int currentPos = Player.position;
                var cachedInfos = Directory.GetFiles(path, "*.mp3");
                if (cachedInfos.Length > 0)
                {
                    foreach (string musicFile in cachedInfos)
                    {
                        Common.populate = true;
                        PopulateMusicFileInfo(musicFile);
                    }
                    Common.populate = true;
                    PopulateMusicFileInfo(Common.cachedInfos[Common.currentPos - 1].MusicPath);
                    PlaybackPositioningTools.SeekToFrame(currentPos);
                }
            }
            else
                InfoBoxColor.WriteInfoBox("Music library directory is not found.");
        }

        internal static void PopulateMusicFileInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !Common.populate)
                return;
            Common.populate = false;
            if (Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                var instance = Common.cachedInfos.Single((csi) => csi.MusicPath == musicPath);
                Player.total = instance.Duration;
                Player.formatInfo = instance.FormatInfo;
                Player.totalSpan = AudioInfoTools.GetDurationSpanFromSamples(Player.total, Player.formatInfo.rate);
                Player.frameInfo = instance.FrameInfo;
                Player.managedV1 = instance.MetadataV1;
                Player.managedV2 = instance.MetadataV2;
                Player.lyricInstance = instance.LyricInstance;
            }
            else
            {
                InfoBoxColor.WriteInfoBox($"Loading BassBoom to open {musicPath}...", false);
                if (FileTools.IsOpened)
                    FileTools.CloseFile();
                FileTools.OpenFile(musicPath);
                Player.total = AudioInfoTools.GetDuration(true);
                Player.totalSpan = AudioInfoTools.GetDurationSpanFromSamples(Player.total);
                Player.formatInfo = FormatTools.GetFormatInfo();
                Player.frameInfo = AudioInfoTools.GetFrameInfo();
                AudioInfoTools.GetId3Metadata(out Player.managedV1, out Player.managedV2);

                // Try to open the lyrics
                OpenLyrics(musicPath);
                var instance = new CachedSongInfo(musicPath, Player.managedV1, Player.managedV2, Player.total, Player.formatInfo, Player.frameInfo, Player.lyricInstance, "", false);
                Common.cachedInfos.Add(instance);
            }
            TextWriterWhereColor.WriteWhere(new string(' ', ConsoleWrapper.WindowWidth), 0, 1);
        }

        internal static string RenderSongName(string musicPath)
        {
            // Render the song name
            var (musicName, musicArtist, _) = GetMusicNameArtistGenre(musicPath);

            // Print the music name
            return CenteredTextColor.RenderCentered(1, "Now playing: {0} - {1}", ConsoleColors.White, ConsoleColors.Black, musicArtist, musicName);
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
            var cachedInfo = Common.cachedInfos[cachedInfoIdx];
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
                InfoBoxColor.WriteInfoBox($"Trying to open lyrics file {lyricsPath}...", false);
                if (File.Exists(lyricsPath))
                    Player.lyricInstance = LyricReader.GetLyrics(lyricsPath);
                else
                    Player.lyricInstance = null;
            }
            catch (Exception ex)
            {
                InfoBoxColor.WriteInfoBox($"Can't open lyrics file {lyricsPath}... {ex.Message}");
            }
        }

        internal static void RemoveCurrentSong()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Common.cachedInfos.RemoveAt(Common.currentPos - 1);
            if (Common.cachedInfos.Count > 0)
            {
                Common.currentPos--;
                if (Common.currentPos == 0)
                    Common.currentPos = 1;
                Common.populate = true;
                PopulateMusicFileInfo(Common.cachedInfos[Common.currentPos - 1].MusicPath);
            }
        }

        internal static void RemoveAllSongs()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            for (int i = Common.cachedInfos.Count; i > 0; i--)
                RemoveCurrentSong();
        }

        internal static void PromptSeek()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            // Prompt the user to set the current position to the specified time
            string time = InfoBoxInputColor.WriteInfoBoxInput("Write the target position in this format: HH:MM:SS");
            if (TimeSpan.TryParse(time, out TimeSpan duration))
            {
                Player.position = (int)(Common.cachedInfos[Common.currentPos - 1].FormatInfo.rate * duration.TotalSeconds);
                if (Player.position > Player.total)
                    Player.position = Player.total;
                PlaybackPositioningTools.SeekToFrame(Player.position);
            }
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
                
                Native State
                ============

                Accurate rendering: {{PlaybackTools.GetNativeState(PlaybackStateType.Accurate)}}
                Buffer fill: {{PlaybackTools.GetNativeState(PlaybackStateType.BufferFill)}}
                Decoding delay: {{PlaybackTools.GetNativeState(PlaybackStateType.DecodeDelay)}}
                Encoding delay: {{PlaybackTools.GetNativeState(PlaybackStateType.EncodeDelay)}}
                Encoding padding: {{PlaybackTools.GetNativeState(PlaybackStateType.EncodePadding)}}
                Frankenstein stream: {{PlaybackTools.GetNativeState(PlaybackStateType.Frankenstein)}}
                Fresh decoder: {{PlaybackTools.GetNativeState(PlaybackStateType.FreshDecoder)}}

                Texts and Extras
                ================

                {{textsBuilder}}
                """
            );
        }
    }
}
