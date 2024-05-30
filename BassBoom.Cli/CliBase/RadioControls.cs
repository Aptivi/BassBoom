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
using BassBoom.Cli.Tools;
using System.Linq;
using System.Threading;
using Terminaux.Base;
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

            // There could be a chance that the music has fully stopped without any user interaction, but since we're on
            // a radio station, we should seek nothing; just drop.
            if (PlaybackTools.State == PlaybackState.Stopped)
                PlaybackPositioningTools.Drop();
            Common.advance = true;
            Radio.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.Playing || Common.failedToPlay);
            Common.failedToPlay = false;
        }

        internal static void Pause()
        {
            Common.advance = false;
            Common.paused = true;
            PlaybackTools.Pause();
        }

        internal static void Stop(bool resetCurrentStation = true)
        {
            Common.advance = false;
            Common.paused = false;
            if (resetCurrentStation)
                Common.currentPos = 1;
            PlaybackTools.Stop();
        }

        internal static void NextStation()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop();
            Common.currentPos++;
            if (Common.currentPos > Common.cachedInfos.Count)
                Common.currentPos = 1;
        }

        internal static void PreviousStation()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop();
            Common.currentPos--;
            if (Common.currentPos <= 0)
                Common.currentPos = Common.cachedInfos.Count;
        }

        internal static void PromptForAddStation()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the radio station. The URL to the station must provide an MPEG radio station. AAC ones are not supported yet.");
            Common.populate = true;
            PopulateRadioStationInfo(path);
            Common.populate = true;
            PopulateRadioStationInfo(Common.cachedInfos[Common.currentPos - 1].MusicPath);
        }

        internal static void PopulateRadioStationInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !Common.populate)
                return;
            Common.populate = false;
            if (Common.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                var instance = Common.cachedInfos.Single((csi) => csi.MusicPath == musicPath);
                Radio.formatInfo = instance.FormatInfo;
                Radio.frameInfo = instance.FrameInfo;
            }
            else
            {
                InfoBoxColor.WriteInfoBox($"Loading BassBoom to open {musicPath}...", false);
                if (FileTools.IsOpened)
                    FileTools.CloseFile();
                FileTools.OpenUrl(musicPath);
                Radio.formatInfo = FormatTools.GetFormatInfo();
                Radio.frameInfo = AudioInfoTools.GetFrameInfo();

                // Try to open the lyrics
                var instance = new CachedSongInfo(musicPath, null, null, -1, Radio.formatInfo, Radio.frameInfo, null, FileTools.CurrentFile.StationName, true);
                Common.cachedInfos.Add(instance);
            }
            TextWriterWhereColor.WriteWhere(new string(' ', ConsoleWrapper.WindowWidth), 0, 1);
        }

        internal static string RenderStationName()
        {
            // Render the station name
            string icy = PlaybackTools.RadioNowPlaying;

            // Print the music name
            return CenteredTextColor.RenderCentered(1, "Now playing: {0}", ConsoleColors.White, ConsoleColors.Black, icy);
        }

        internal static void RemoveCurrentStation()
        {
            // In case we have no stations in the playlist...
            if (Common.cachedInfos.Count == 0)
                return;

            Common.cachedInfos.RemoveAt(Common.currentPos - 1);
            if (Common.cachedInfos.Count > 0)
            {
                Common.currentPos--;
                if (Common.currentPos == 0)
                    Common.currentPos = 1;
                Common.populate = true;
                PopulateRadioStationInfo(Common.cachedInfos[Common.currentPos - 1].MusicPath);
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
            InfoBoxColor.WriteInfoBox(
                $$"""
                Station info
                =========

                Radio station URL: {{Common.cachedInfos[Common.currentPos - 1].MusicPath}}
                Radio station name: {{Common.cachedInfos[Common.currentPos - 1].StationName}}
                Radio station current song: {{PlaybackTools.RadioNowPlaying}}
                
                Layer info
                ==========

                Version: {{Radio.frameInfo.Version}}
                Layer: {{Radio.frameInfo.Layer}}
                Rate: {{Radio.frameInfo.Rate}}
                Mode: {{Radio.frameInfo.Mode}}
                Mode Ext: {{Radio.frameInfo.ModeExt}}
                Frame Size: {{Radio.frameInfo.FrameSize}}
                Flags: {{Radio.frameInfo.Flags}}
                Emphasis: {{Radio.frameInfo.Emphasis}}
                Bitrate: {{Radio.frameInfo.BitRate}}
                ABR Rate: {{Radio.frameInfo.AbrRate}}
                VBR: {{Radio.frameInfo.Vbr}}
                
                Native State
                ============
                
                Accurate rendering: {{PlaybackTools.GetNativeState(PlaybackStateType.Accurate)}}
                Buffer fill: {{PlaybackTools.GetNativeState(PlaybackStateType.BufferFill)}}
                Decoding delay: {{PlaybackTools.GetNativeState(PlaybackStateType.DecodeDelay)}}
                Encoding delay: {{PlaybackTools.GetNativeState(PlaybackStateType.EncodeDelay)}}
                Encoding padding: {{PlaybackTools.GetNativeState(PlaybackStateType.EncodePadding)}}
                Frankenstein stream: {{PlaybackTools.GetNativeState(PlaybackStateType.Frankenstein)}}
                Fresh decoder: {{PlaybackTools.GetNativeState(PlaybackStateType.FreshDecoder)}}
                """
            );
        }
    }
}
