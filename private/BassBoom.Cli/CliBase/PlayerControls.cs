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

using BassBoom.Basolia.Enumerations;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Tools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base.Buffered;
using Terminaux.Base.Extensions;
using Terminaux.Colors.Data;
using Terminaux.Inputs;
using Terminaux.Inputs.Styles;
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
            if (Common.CurrentCachedInfo is null)
                return;

            Player.position += (int)(Common.CurrentCachedInfo.FormatInfo.rate * seekRate);
            if (Player.position > Common.CurrentCachedInfo.Duration)
                Player.position = Common.CurrentCachedInfo.Duration;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void SeekBackward()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;

            Player.position -= (int)(Common.CurrentCachedInfo.FormatInfo.rate * seekRate);
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

        internal static void SeekPreviousLyric()
        {
            // In case we have no songs in the playlist, or we have no lyrics...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;
            if (Common.CurrentCachedInfo.LyricInstance is null)
                return;

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesCurrent();
            if (lyrics.Length == 0)
                return;
            var lyric = lyrics.Length == 1 ? lyrics[0] : lyrics[lyrics.Length - 2];
            PlaybackPositioningTools.SeekLyric(lyric);
        }

        internal static void SeekCurrentLyric()
        {
            // In case we have no songs in the playlist, or we have no lyrics...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;
            if (Common.CurrentCachedInfo.LyricInstance is null)
                return;

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesCurrent();
            if (lyrics.Length == 0)
                return;
            var lyric = lyrics[lyrics.Length - 1];
            PlaybackPositioningTools.SeekLyric(lyric);
        }

        internal static void SeekNextLyric()
        {
            // In case we have no songs in the playlist, or we have no lyrics...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;
            if (Common.CurrentCachedInfo.LyricInstance is null)
                return;

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesUpcoming();
            if (lyrics.Length == 0)
            {
                SeekCurrentLyric();
                return;
            }
            var lyric = lyrics[0];
            PlaybackPositioningTools.SeekLyric(lyric);
        }

        internal static void SeekWhichLyric()
        {
            // In case we have no songs in the playlist, or we have no lyrics...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;
            if (Common.CurrentCachedInfo.LyricInstance is null)
                return;

            var lyrics = Common.CurrentCachedInfo.LyricInstance.Lines;
            var choices = lyrics.Select((line) => new InputChoiceInfo($"{line.LineSpan}", line.Line)).ToArray();
            int index = InfoBoxSelectionColor.WriteInfoBoxSelection(choices, "Select a lyric to seek to");
            if (index == -1)
                return;
            var lyric = lyrics[index];
            PlaybackPositioningTools.SeekLyric(lyric);
        }

        internal static void SeekTo(TimeSpan target)
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;

            Player.position = (int)(target.TotalSeconds * Common.CurrentCachedInfo.FormatInfo.rate);
            if (Player.position > Common.CurrentCachedInfo.Duration)
                Player.position = 0;
            PlaybackPositioningTools.SeekToFrame(Player.position);
        }

        internal static void Play()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Player.playerThread is null)
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
            ScreenTools.CurrentScreen?.RequireRefresh();
            if (File.Exists(path))
            {
                int currentPos = Player.position;
                Common.populate = true;
                PopulateMusicFileInfo(path);
                Common.populate = true;
                PopulateMusicFileInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                PlaybackPositioningTools.SeekToFrame(currentPos);
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal($"File \"{path}\" doesn't exist.");
        }

        internal static void PromptForAddDirectory()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the music library directory");
            ScreenTools.CurrentScreen?.RequireRefresh();
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
                    PopulateMusicFileInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                    PlaybackPositioningTools.SeekToFrame(currentPos);
                }
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal("Music library directory is not found.");
        }

        internal static void PopulateMusicFileInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !Common.populate)
                return;
            Common.populate = false;
            Common.Switch(musicPath);
            if (!Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                ScreenTools.CurrentScreen?.RequireRefresh();
                InfoBoxNonModalColor.WriteInfoBox($"Loading BassBoom to open {musicPath}...");
                var total = AudioInfoTools.GetDuration(true);
                var formatInfo = FormatTools.GetFormatInfo();
                var frameInfo = AudioInfoTools.GetFrameInfo();
                AudioInfoTools.GetId3Metadata(out var managedV1, out var managedV2);

                // Try to open the lyrics
                var lyric = OpenLyrics(musicPath);
                var instance = new CachedSongInfo(musicPath, managedV1, managedV2, total, formatInfo, frameInfo, lyric, "", false);
                Common.cachedInfos.Add(instance);
            }
        }

        internal static string RenderSongName(string musicPath)
        {
            // Render the song name
            var (musicName, musicArtist, _) = GetMusicNameArtistGenre(musicPath);

            // Print the music name
            return $"Now playing: {musicArtist} - {musicName}";
        }

        internal static (string musicName, string musicArtist, string musicGenre) GetMusicNameArtistGenre(string musicPath)
        {
            if (Common.CurrentCachedInfo is null)
                return ("", "", "");
            var metadatav2 = Common.CurrentCachedInfo.MetadataV2;
            var metadatav1 = Common.CurrentCachedInfo.MetadataV1;
            string musicName =
                (!string.IsNullOrEmpty(metadatav2?.Title) ? metadatav2?.Title :
                 !string.IsNullOrEmpty(metadatav1?.Title) ? metadatav1?.Title :
                 Path.GetFileNameWithoutExtension(musicPath)) ?? "";
            string musicArtist =
                (!string.IsNullOrEmpty(metadatav2?.Artist) ? metadatav2?.Artist :
                 !string.IsNullOrEmpty(metadatav1?.Artist) ? metadatav1?.Artist :
                 "Unknown Artist") ?? "";
            string musicGenre =
                (!string.IsNullOrEmpty(metadatav2?.Genre) ? metadatav2?.Genre :
                 metadatav1?.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                 "Unknown Genre") ?? "";
            return (musicName, musicArtist, musicGenre);
        }

        internal static (string musicName, string musicArtist, string musicGenre) GetMusicNameArtistGenre(int cachedInfoIdx)
        {
            var cachedInfo = Common.cachedInfos[cachedInfoIdx];
            var metadatav2 = cachedInfo.MetadataV2;
            var metadatav1 = cachedInfo.MetadataV1;
            var path = cachedInfo.MusicPath;
            string musicName =
                (!string.IsNullOrEmpty(metadatav2?.Title) ? metadatav2?.Title :
                 !string.IsNullOrEmpty(metadatav1?.Title) ? metadatav1?.Title :
                 Path.GetFileNameWithoutExtension(path)) ?? "";
            string musicArtist =
                (!string.IsNullOrEmpty(metadatav2?.Artist) ? metadatav2?.Artist :
                 !string.IsNullOrEmpty(metadatav1?.Artist) ? metadatav1?.Artist :
                 "Unknown Artist") ?? "";
            string musicGenre =
                (!string.IsNullOrEmpty(metadatav2?.Genre) ? metadatav2?.Genre :
                 metadatav1?.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                 "Unknown Genre") ?? "";
            return (musicName, musicArtist, musicGenre);
        }

        internal static Lyric? OpenLyrics(string musicPath)
        {
            string lyricsPath = Path.GetDirectoryName(musicPath) + "/" + Path.GetFileNameWithoutExtension(musicPath) + ".lrc";
            try
            {
                InfoBoxNonModalColor.WriteInfoBox($"Trying to open lyrics file {lyricsPath}...");
                if (File.Exists(lyricsPath))
                    return LyricReader.GetLyrics(lyricsPath);
                else
                    return null;
            }
            catch (Exception ex)
            {
                InfoBoxModalColor.WriteInfoBoxModal($"Can't open lyrics file {lyricsPath}... {ex.Message}");
            }
            return null;
        }

        internal static void RemoveCurrentSong()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Common.CurrentCachedInfo is null)
                return;

            Common.cachedInfos.RemoveAt(Common.currentPos - 1);
            if (Common.cachedInfos.Count > 0)
            {
                Common.currentPos--;
                if (Common.currentPos == 0)
                    Common.currentPos = 1;
                Common.populate = true;
                PopulateMusicFileInfo(Common.CurrentCachedInfo.MusicPath);
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
            if (Common.CurrentCachedInfo is null)
                return;

            // Prompt the user to set the current position to the specified time
            string time = InfoBoxInputColor.WriteInfoBoxInput("Write the target position in this format: HH:MM:SS");
            if (TimeSpan.TryParse(time, out TimeSpan duration))
            {
                Player.position = (int)(Common.CurrentCachedInfo.FormatInfo.rate * duration.TotalSeconds);
                if (Player.position > Common.CurrentCachedInfo.Duration)
                    Player.position = Common.CurrentCachedInfo.Duration;
                PlaybackPositioningTools.SeekToFrame(Player.position);
            }
        }

        internal static void ShowSongInfo()
        {
            if (Common.CurrentCachedInfo is null)
                return;
            var textsBuilder = new StringBuilder();
            var idv2 = Common.CurrentCachedInfo.MetadataV2;
            var idv1 = Common.CurrentCachedInfo.MetadataV1;
            foreach (var text in idv2?.Texts ?? [])
                textsBuilder.AppendLine($"T - {text.Item1}: {text.Item2}");
            foreach (var text in idv2?.Extras ?? [])
                textsBuilder.AppendLine($"E - {text.Item1}: {text.Item2}");
            InfoBoxModalColor.WriteInfoBoxModal(
                $$"""
                Song info
                =========

                Artist: {{(!string.IsNullOrEmpty(idv2?.Artist) ? idv2?.Artist : !string.IsNullOrEmpty(idv1?.Artist) ? idv1?.Artist : "Unknown")}}
                Title: {{(!string.IsNullOrEmpty(idv2?.Title) ? idv2?.Title : !string.IsNullOrEmpty(idv1?.Title) ? idv1?.Title : "")}}
                Album: {{(!string.IsNullOrEmpty(idv2?.Album) ? idv2?.Album : !string.IsNullOrEmpty(idv1?.Album) ? idv1?.Album : "")}}
                Genre: {{(!string.IsNullOrEmpty(idv2?.Genre) ? idv2?.Genre : !string.IsNullOrEmpty(idv1?.Genre.ToString()) ? idv1?.Genre.ToString() : "")}}
                Comment: {{(!string.IsNullOrEmpty(idv2?.Comment) ? idv2?.Comment : !string.IsNullOrEmpty(idv1?.Comment) ? idv1?.Comment : "")}}
                Duration: {{Common.CurrentCachedInfo.DurationSpan}}
                Lyrics: {{(Common.CurrentCachedInfo.LyricInstance is not null ? $"{Common.CurrentCachedInfo.LyricInstance.Lines.Count} lines" : "No lyrics")}}
                
                Layer info
                ==========

                Version: {{Common.CurrentCachedInfo.FrameInfo.Version}}
                Layer: {{Common.CurrentCachedInfo.FrameInfo.Layer}}
                Rate: {{Common.CurrentCachedInfo.FrameInfo.Rate}}
                Mode: {{Common.CurrentCachedInfo.FrameInfo.Mode}}
                Mode Ext: {{Common.CurrentCachedInfo.FrameInfo.ModeExt}}
                Frame Size: {{Common.CurrentCachedInfo.FrameInfo.FrameSize}}
                Flags: {{Common.CurrentCachedInfo.FrameInfo.Flags}}
                Emphasis: {{Common.CurrentCachedInfo.FrameInfo.Emphasis}}
                Bitrate: {{Common.CurrentCachedInfo.FrameInfo.BitRate}}
                ABR Rate: {{Common.CurrentCachedInfo.FrameInfo.AbrRate}}
                VBR: {{Common.CurrentCachedInfo.FrameInfo.Vbr}}
                
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
