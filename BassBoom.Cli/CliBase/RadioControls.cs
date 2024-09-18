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
using BassBoom.Basolia.Playback;
using BassBoom.Basolia.Radio;
using BassBoom.Cli.Tools;
using System.Linq;
using System.Text;
using System.Threading;
using Terminaux.Base.Buffered;
using Terminaux.Base.Extensions;
using Terminaux.Colors.Data;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;

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

        internal static void PopulateRadioStationInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.IsPlaying(BassBoomCli.basolia) || !Common.populate)
                return;
            Common.populate = false;
            Common.Switch(musicPath);
            if (!Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                InfoBoxColor.WriteInfoBox($"Opening {musicPath}...", false);
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
            InfoBoxColor.WriteInfoBox(
                $$"""
                Station info
                ============

                Radio station URL: {{Common.CurrentCachedInfo.MusicPath}}
                Radio station name: {{Common.CurrentCachedInfo.StationName}}
                Radio station current song: {{PlaybackTools.GetRadioNowPlaying(BassBoomCli.basolia)}}
                
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
                
                Accurate rendering: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Accurate)}}
                Buffer fill: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.BufferFill)}}
                Decoding delay: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.DecodeDelay)}}
                Encoding delay: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodeDelay)}}
                Encoding padding: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.EncodePadding)}}
                Frankenstein stream: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.Frankenstein)}}
                Fresh decoder: {{PlaybackTools.GetNativeState(BassBoomCli.basolia, PlaybackStateType.FreshDecoder)}}
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
                InfoBoxColor.WriteInfoBox(
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
                InfoBoxColor.WriteInfoBox($"Unable to get extended radio station info for {Common.CurrentCachedInfo.MusicPath}");
        }
    }
}
