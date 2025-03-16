﻿//
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
            // a radio station, we should seek nothing.
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
                InfoBoxNonModalColor.WriteInfoBox($"Opening {musicPath}...", false);

                // Try to open the lyrics
                var instance = new CachedSongInfo(musicPath, -1, null, FileTools.CurrentFile(BassBoomCli.basolia)?.StationName ?? "", true, null);
                Common.cachedInfos.Add(instance);
            }
        }

        internal static string RenderStationName()
        {
            // Render the station name
            string icy = PlaybackTools.GetRadioNowPlaying(BassBoomCli.basolia);

            // Print the music name
            return $"Now playing: {icy}";
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
                $$"""
                Station info
                ============

                Radio station URL: {{Common.CurrentCachedInfo.MusicPath}}
                Radio station name: {{Common.CurrentCachedInfo.StationName}}
                Radio station current song: {{PlaybackTools.GetRadioNowPlaying(BassBoomCli.basolia)}}
                """
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
                    streamBuilder.AppendLine($"Name: {stream.StreamTitle}");
                    streamBuilder.AppendLine($"Home page: {stream.StreamHomepage}");
                    streamBuilder.AppendLine($"Genre: {stream.StreamGenre}");
                    streamBuilder.AppendLine($"Now playing: {stream.SongTitle}");
                    streamBuilder.AppendLine($"Stream path: {stream.StreamPath}");
                    streamBuilder.AppendLine($"Listeners: {stream.CurrentListeners} with {stream.PeakListeners} at peak");
                    streamBuilder.AppendLine($"Bit rate: {stream.BitRate} kbps");
                    streamBuilder.AppendLine($"Media type: {stream.MimeInfo}");
                    streamBuilder.AppendLine("===============================");
                }
                InfoBoxModalColor.WriteInfoBoxModal(
                    $$"""
                    Radio server info
                    =================

                    Radio station URL: {{station.ServerHostFull}}
                    Radio station uses HTTPS: {{station.ServerHttps}}
                    Radio station server type: {{station.ServerType}}
                    Radio station streams: {{station.TotalStreams}} with {{station.ActiveStreams}} active
                    Radio station listeners: {{station.CurrentListeners}} with {{station.PeakListeners}} at peak
                
                    Stream info
                    ===========

                    ===============================
                    {{streamBuilder}}
                    """
                );
            }
            else
                InfoBoxModalColor.WriteInfoBoxModal($"Unable to get extended radio station info for {Common.CurrentCachedInfo.MusicPath}");
        }
    }
}
