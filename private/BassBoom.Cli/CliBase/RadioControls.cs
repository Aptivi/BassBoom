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
using BassBoom.Basolia.Playback;
using BassBoom.Basolia.Playback.Playlists;
using BassBoom.Basolia.Playback.Playlists.Enumerations;
using BassBoom.Basolia.Radio;
using BassBoom.Cli.Tools;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base.Buffered;
using Terminaux.Inputs.Styles.Infobox;
using Textify.General;

namespace BassBoom.Cli.CliBase
{
    internal static class RadioControls
    {
        internal static void Play()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;
            if (Radio.playerThread is null)
                return;

            // There could be a chance that the music has fully stopped without any user interaction, but since we're on
            // a radio station, we should seek nothing; just drop.
            if (PlaybackTools.GetState(BassBoomCli.basolia) == PlaybackState.Stopped)
                PlaybackPositioningTools.Drop(BassBoomCli.basolia);
            Common.advance = true;
            Radio.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.IsPlaying(BassBoomCli.basolia) || Common.failedToPlay);
            Common.failedToPlay = false;
        }

        internal static void Pause()
        {
            Common.advance = false;
            Common.paused = true;
            PlaybackTools.Pause(BassBoomCli.basolia);
        }

        internal static void Stop(bool resetCurrentStation = true)
        {
            Common.advance = false;
            Common.paused = false;
            if (resetCurrentStation)
                Common.currentPos = 1;
            PlaybackTools.Stop(BassBoomCli.basolia);
        }

        internal static void NextStation()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop(BassBoomCli.basolia);
            Common.currentPos++;
            if (Common.currentPos > Common.cachedInfos.Count)
                Common.currentPos = 1;
        }

        internal static void PreviousStation()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop(BassBoomCli.basolia);
            Common.currentPos--;
            if (Common.currentPos <= 0)
                Common.currentPos = Common.cachedInfos.Count;
        }

        internal static void PromptForAddStation()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the radio station. The URL to the station must provide an MPEG radio station. AAC ones are not supported yet.");
            ScreenTools.CurrentScreen?.RequireRefresh();
            Common.populate = true;
            PopulateRadioStationInfo(path);
            Common.populate = true;
            PopulateRadioStationInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
        }

        internal static void PromptForAddStations()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the playlist of radio stations. The URLs to the stations must provide an MPEG radio station. AAC ones are not supported yet.");
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
                        if (track.Type == SongType.Radio)
                        {
                            Common.populate = true;
                            PopulateRadioStationInfo(track.Path);
                        }
                    }
                    Common.populate = true;
                    PopulateRadioStationInfo(Common.CurrentCachedInfo?.MusicPath ?? "");
                }
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal("Radio station playlist is not found.");
        }

        internal static void PopulateRadioStationInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.IsPlaying(BassBoomCli.basolia) || !Common.populate)
                return;
            Common.populate = false;
            Common.Switch(musicPath);
            if (!Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                InfoBoxNonModalColor.WriteInfoBox("Opening {0}...".FormatString(musicPath), false);
                var formatInfo = FormatTools.GetFormatInfo(BassBoomCli.basolia);
                var frameInfo = AudioInfoTools.GetFrameInfo(BassBoomCli.basolia);

                // Try to open the lyrics
                var instance = new CachedSongInfo(musicPath, null, null, -1, formatInfo, frameInfo, null, FileTools.CurrentFile(BassBoomCli.basolia)?.StationName ?? "", true);
                Common.cachedInfos.Add(instance);
            }
        }

        internal static string RenderStationName()
        {
            // Render the station name
            string icy = PlaybackTools.GetRadioNowPlaying(BassBoomCli.basolia);

            // Print the music name
            return "Now playing:" + $" {icy}";
        }

        internal static void RemoveCurrentStation()
        {
            // In case we have no stations in the playlist...
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
                PopulateRadioStationInfo(Common.CurrentCachedInfo.MusicPath);
            }
        }

        internal static void RemoveAllStations()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            for (int i = Common.cachedInfos.Count; i > 0; i--)
                RemoveCurrentStation();
        }

        internal static void ShowStationInfo()
        {
            if (Common.CurrentCachedInfo is null)
                return;
            InfoBoxModalColor.WriteInfoBoxModal(
                "Station info" + "\n\n" +
                "Radio station URL:" + $" {Common.CurrentCachedInfo.MusicPath}" + "\n" +
                "Radio station name:" + $" {Common.CurrentCachedInfo.StationName}" + "\n" +
                "Radio station current song:" + $" {PlaybackTools.GetRadioNowPlaying(BassBoomCli.basolia)}" + "\n\n" +

                "Layer info" + "\n\n" +
                "Version:" + $" {Common.CurrentCachedInfo.FrameInfo.Version}" + "\n" +
                "Layer:" + $" {Common.CurrentCachedInfo.FrameInfo.Layer}" + "\n" +
                "Rate:" + $" {Common.CurrentCachedInfo.FrameInfo.Rate}" + "\n" +
                "Mode:" + $" {Common.CurrentCachedInfo.FrameInfo.Mode}" + "\n" +
                "Mode Ext:" + $" {Common.CurrentCachedInfo.FrameInfo.ModeExt}" + "\n" +
                "Frame Size:" + $" {Common.CurrentCachedInfo.FrameInfo.FrameSize}" + "\n" +
                "Flags:" + $" {Common.CurrentCachedInfo.FrameInfo.Flags}" + "\n" +
                "Emphasis:" + $" {Common.CurrentCachedInfo.FrameInfo.Emphasis}" + "\n" +
                "Bitrate:" + $" {Common.CurrentCachedInfo.FrameInfo.BitRate}" + "\n" +
                "ABR Rate:" + $" {Common.CurrentCachedInfo.FrameInfo.AbrRate}" + "\n" +
                "VBR:" + $" {Common.CurrentCachedInfo.FrameInfo.Vbr}" + "\n\n" +

                "Native State" + "\n\n" +
                "Accurate rendering:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Accurate)}" + "\n" +
                "Buffer fill:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.BufferFill)}" + "\n" +
                "Decoding delay:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.DecodeDelay)}" + "\n" +
                "Encoding delay:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodeDelay)}" + "\n" +
                "Encoding padding:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodePadding)}" + "\n" +
                "Frankenstein stream:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Frankenstein)}" + "\n" +
                "Fresh decoder:" + $" {PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.FreshDecoder)}"
            );
        }

        internal static void ShowExtendedStationInfo()
        {
            if (Common.CurrentCachedInfo is null)
                return;
            var station = RadioTools.GetRadioInfo(Common.CurrentCachedInfo.MusicPath);
            var streamBuilder = new StringBuilder();
            if (station is not null)
            {
                foreach (var stream in station.Streams)
                {
                    streamBuilder.AppendLine("Name:" + $" {stream.StreamTitle}");
                    streamBuilder.AppendLine("    " + "Home page:" + $" {stream.StreamHomepage}");
                    streamBuilder.AppendLine("    " + "Genre:" + $" {stream.StreamGenre}");
                    streamBuilder.AppendLine("    " + "Now playing:" + $" {stream.SongTitle}");
                    streamBuilder.AppendLine("    " + "Stream path:" + $" {stream.StreamPath}");
                    streamBuilder.AppendLine("    " + "Listeners: {0} with {1} at peak".FormatString(stream.CurrentListeners, stream.PeakListeners));
                    streamBuilder.AppendLine("    " + "Bit rate:" + $" {stream.BitRate} kbps");
                    streamBuilder.AppendLine("    " + "Media type:" + $" {stream.MimeInfo}");
                    streamBuilder.AppendLine();
                }
                InfoBoxModalColor.WriteInfoBoxModal(
                    "Radio server info" + "\n\n" +
                    "Radio station URL:" + $" {station.ServerHostFull}" + "\n" +
                    "Radio station uses HTTPS:" + $" {station.ServerHttps}" + "\n" +
                    "Radio station server type:" + $" {station.ServerType}" + "\n" +
                    "Radio station streams: {0} with {1} active".FormatString(station.TotalStreams, station.ActiveStreams) + "\n" +
                    "Radio station listeners: {0} with {1} at peak".FormatString(station.CurrentListeners, station.PeakListeners) + "\n\n" +

                    "Stream info" + "\n\n" +
                    streamBuilder.ToString()
                );
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal("Unable to get extended radio station info for {0}".FormatString(Common.CurrentCachedInfo.MusicPath));
        }
    }
}
