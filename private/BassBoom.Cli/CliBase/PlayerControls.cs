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
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Independent;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using BassBoom.Basolia.Playback.Playlists;
using BassBoom.Basolia.Playback.Playlists.Enumerations;
using BassBoom.Cli.Languages;
using BassBoom.Cli.Tools;
using SpecProbe.Software.Platform;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base.Buffered;
using Terminaux.Inputs.Styles;
using Terminaux.Inputs.Styles.Infobox;
using Textify.General;

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
            PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, Player.position);
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
            PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, Player.position);
        }

        internal static void SeekBeginning()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackPositioningTools.SeekToTheBeginning(BassBoomCli.basolia);
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

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesCurrent(BassBoomCli.basolia);
            if (lyrics.Length == 0)
                return;
            var lyric = lyrics.Length == 1 ? lyrics[0] : lyrics[lyrics.Length - 2];
            PlaybackPositioningTools.SeekLyric(BassBoomCli.basolia, lyric);
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

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesCurrent(BassBoomCli.basolia);
            if (lyrics.Length == 0)
                return;
            var lyric = lyrics[lyrics.Length - 1];
            PlaybackPositioningTools.SeekLyric(BassBoomCli.basolia, lyric);
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

            var lyrics = Common.CurrentCachedInfo.LyricInstance.GetLinesUpcoming(BassBoomCli.basolia);
            if (lyrics.Length == 0)
            {
                SeekCurrentLyric();
                return;
            }
            var lyric = lyrics[0];
            PlaybackPositioningTools.SeekLyric(BassBoomCli.basolia, lyric);
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
            int index = InfoBoxSelectionColor.WriteInfoBoxSelection(choices, LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_SELECTLYRICSEEK"));
            if (index == -1)
                return;
            var lyric = lyrics[index];
            PlaybackPositioningTools.SeekLyric(BassBoomCli.basolia, lyric);
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
            PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, Player.position);
        }

        internal static void Play()
        {
            // In case we have no songs in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Player.playerThread is null)
                return;

            if (PlaybackTools.GetState(BassBoomCli.basolia) == PlaybackState.Stopped)
                // There could be a chance that the music has fully stopped without any user interaction.
                PlaybackPositioningTools.SeekToTheBeginning(BassBoomCli.basolia);
            Common.advance = true;
            Player.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.IsPlaying(BassBoomCli.basolia) || Common.failedToPlay);
            Common.failedToPlay = false;
        }

        internal static void Pause()
        {
            Common.advance = false;
            Common.paused = true;
            PlaybackTools.Pause(BassBoomCli.basolia);
        }

        internal static void Stop(bool resetCurrentSong = true)
        {
            Common.advance = false;
            Common.paused = false;
            if (resetCurrentSong)
                Common.currentPos = 1;
            PlaybackTools.Stop(BassBoomCli.basolia);
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
            string path = InfoBoxInputColor.WriteInfoBoxInput(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICFILEPROMPT"));
            ScreenTools.CurrentScreen?.RequireRefresh();
            if (File.Exists(path))
            {
                int currentPos = Player.position;
                Common.populate = true;
                PopulateMusicFileInfo(path);
                Common.populate = true;
                PopulateMusicFileInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, currentPos);
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICFILENOTFOUND").FormatString(path));
        }

        internal static void PromptForAddSongs()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICPLAYLISTPROMPT"));
            string extension = Path.GetExtension(path);
            ScreenTools.CurrentScreen?.RequireRefresh();
            if (File.Exists(path) && (extension == ".m3u" || extension == ".m3u8"))
            {
                int currentPos = Player.position;
                var playlist = PlaylistParser.ParsePlaylist(path);
                if (playlist.Tracks.Length > 0)
                {
                    foreach (var track in playlist.Tracks)
                    {
                        if (track.Type == SongType.File)
                        {
                            Common.populate = true;
                            PopulateMusicFileInfo(track.Path);
                        }
                    }
                    Common.populate = true;
                    PopulateMusicFileInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                    PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, currentPos);
                }
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICPLAYLISTNOTFOUND"));
        }

        internal static void PromptForAddDirectory()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICLIBRARYPROMPT"));
            ScreenTools.CurrentScreen?.RequireRefresh();
            if (Directory.Exists(path))
            {
                int currentPos = Player.position;
                var cachedInfos = Directory.EnumerateFiles(path).Where((pathStr) => FileTools.SupportedExtensions.Contains(Path.GetExtension(pathStr))).ToArray();
                if (cachedInfos.Length > 0)
                {
                    foreach (string musicFile in cachedInfos)
                    {
                        Common.populate = true;
                        PopulateMusicFileInfo(musicFile);
                    }
                    Common.populate = true;
                    PopulateMusicFileInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                    PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, currentPos);
                }
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_MUSICLIBRARYNOTFOUND"));
        }

        internal static void PopulateMusicFileInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.IsPlaying(BassBoomCli.basolia) || !Common.populate)
                return;
            Common.populate = false;
            Common.Switch(musicPath);
            if (!Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                ScreenTools.CurrentScreen?.RequireRefresh();
                InfoBoxNonModalColor.WriteInfoBox(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_OPENINGMUSICFILE").FormatString(musicPath), false);
                var total = AudioInfoTools.GetDuration(BassBoomCli.basolia, true);
                var formatInfo = FormatTools.GetFormatInfo(BassBoomCli.basolia);
                var frameInfo = AudioInfoTools.GetFrameInfo(BassBoomCli.basolia);
                AudioInfoTools.GetId3Metadata(BassBoomCli.basolia, out var managedV1, out var managedV2);

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
            return LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_NOWPLAYING") + $" {musicArtist} - {musicName}";
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
                 LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_UNKNOWNARTIST")) ?? "";
            string musicGenre =
                (!string.IsNullOrEmpty(metadatav2?.Genre) ? metadatav2?.Genre :
                 metadatav1?.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                 LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_UNKNOWNGENRE")) ?? "";
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
                 LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_UNKNOWNARTIST")) ?? "";
            string musicGenre =
                (!string.IsNullOrEmpty(metadatav2?.Genre) ? metadatav2?.Genre :
                 metadatav1?.GenreIndex >= 0 ? $"{metadatav1.Genre} [{metadatav1.GenreIndex}]" :
                 LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_UNKNOWNGENRE")) ?? "";
            return (musicName, musicArtist, musicGenre);
        }

        internal static Lyric? OpenLyrics(string musicPath)
        {
            string lyricsPath = Path.GetDirectoryName(musicPath) + "/" + Path.GetFileNameWithoutExtension(musicPath) + ".lrc";
            try
            {
                InfoBoxNonModalColor.WriteInfoBox(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_OPENINGMUSICLYRICFILE").FormatString(lyricsPath), false);
                if (File.Exists(lyricsPath))
                    return LyricReader.GetLyrics(lyricsPath);
                else
                    return null;
            }
            catch (Exception ex)
            {
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_OPENINGMUSICLYRICFILEFAILED").FormatString(lyricsPath) + $" {ex.Message}");
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
            string time = InfoBoxInputColor.WriteInfoBoxInput(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TARGETPOSPROMPT") + " HH:MM:SS");
            if (TimeSpan.TryParse(time, out TimeSpan duration))
            {
                Player.position = (int)(Common.CurrentCachedInfo.FormatInfo.rate * duration.TotalSeconds);
                if (Player.position > Common.CurrentCachedInfo.Duration)
                    Player.position = Common.CurrentCachedInfo.Duration;
                PlaybackPositioningTools.SeekToFrame(BassBoomCli.basolia, Player.position);
            }
        }

        internal static void PlayTest()
        {
            if (Common.CurrentCachedInfo is not null)
                return;

            // Ignore all settings while playing test sound, because it IS a test session.
            InfoBoxNonModalColor.WriteInfoBox(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDPLAYING"), false);

            // Extract the test sound asset to a temporary file
            var stream = typeof(PlayerControls).Assembly.GetManifestResourceStream("BassBoom.Cli.sample.mp3") ??
                throw new Exception(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_EXCEPTION_TESTSOUNDMISSING"));

            // Now, close the file and play it
            PlayForget.PlayStream(stream);

            // Ask the user if everything is OK.
            int answer = InfoBoxButtonsColor.WriteInfoBoxButtons(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_TITLE"), [new InputChoiceInfo("yes", LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_YES")), new InputChoiceInfo("no", LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_NO"))], LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_DESC"));
            if (answer == 0)
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_SUCCESS"));
            else if (answer == 1)
                InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_TESTSOUNDREVIEW_FAILURE"));
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
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO") + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_ARTIST") + $" {(!string.IsNullOrEmpty(idv2?.Artist) ? idv2?.Artist : !string.IsNullOrEmpty(idv1?.Artist) ? idv1?.Artist : LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_UNKNOWN"))}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_TITLE") + $" {(!string.IsNullOrEmpty(idv2?.Title) ? idv2?.Title : !string.IsNullOrEmpty(idv1?.Title) ? idv1?.Title : "")}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_ALBUM") + $" {(!string.IsNullOrEmpty(idv2?.Album) ? idv2?.Album : !string.IsNullOrEmpty(idv1?.Album) ? idv1?.Album : "")}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_GENRE") + $" {(!string.IsNullOrEmpty(idv2?.Genre) ? idv2?.Genre : !string.IsNullOrEmpty(idv1?.Genre.ToString()) ? idv1?.Genre.ToString() : "")}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_COMMENT") + $" {(!string.IsNullOrEmpty(idv2?.Comment) ? idv2?.Comment : !string.IsNullOrEmpty(idv1?.Comment) ? idv1?.Comment : "")}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_DURATION") + $" {Common.CurrentCachedInfo.DurationSpan}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_LYRICS") + $" {(Common.CurrentCachedInfo.LyricInstance is not null ? LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_LYRICS_LINES").FormatString(Common.CurrentCachedInfo.LyricInstance.Lines.Count) : LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_SONGINFO_LYRICS_NOLYRICS"))}" + "\n\n" +

                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO") + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_VERSION") + $" {Common.CurrentCachedInfo.FrameInfo.Version}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_LAYER") + $" {Common.CurrentCachedInfo.FrameInfo.Layer}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_RATE") + $" {Common.CurrentCachedInfo.FrameInfo.Rate}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_MODE") + $" {Common.CurrentCachedInfo.FrameInfo.Mode}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_MODEEXT") + $" {Common.CurrentCachedInfo.FrameInfo.ModeExt}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_FRAMESIZE") + $" {Common.CurrentCachedInfo.FrameInfo.FrameSize}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_FLAGS") + $" {Common.CurrentCachedInfo.FrameInfo.Flags}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_EMPHASIS") + $" {Common.CurrentCachedInfo.FrameInfo.Emphasis}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_BITRATE") + $" {Common.CurrentCachedInfo.FrameInfo.BitRate}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_ABR") + $" {Common.CurrentCachedInfo.FrameInfo.AbrRate}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_LAYERINFO_VBR") + $" {Common.CurrentCachedInfo.FrameInfo.Vbr}" + "\n\n" +

                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE") + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_ACCURATERENDERING") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Accurate)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_BUFFERFILL") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.BufferFill)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_DECODINGDELAY") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.DecodeDelay)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_ENCODINGDELAY") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodeDelay)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_ENCODINGPADDING") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodePadding)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_FRANKENSTEIN") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Frankenstein)}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_NATIVESTATE_FRESHDECODER") + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.FreshDecoder)}" + "\n\n" +

                LanguageTools.GetLocalized("BASSBOOM_APP_PLAYER_INFO_TEXTSANDEXTRAS") + "\n\n" +
                textsBuilder.ToString()
            );
        }
    }
}
