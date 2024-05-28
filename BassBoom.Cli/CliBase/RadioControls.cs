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

using BassBoom.Basolia;
using BassBoom.Basolia.Devices;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Tools;
using BassBoom.Native.Interop.Analysis;
using SpecProbe.Platform;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        internal static void RaiseVolume()
        {
            Radio.volume += 0.05;
            if (Radio.volume > 1)
                Radio.volume = 1;
            PlaybackTools.SetVolume(Radio.volume);
        }

        internal static void LowerVolume()
        {
            Radio.volume -= 0.05;
            if (Radio.volume < 0)
                Radio.volume = 0;
            PlaybackTools.SetVolume(Radio.volume);
        }

        internal static void Play()
        {
            // In case we have no stations in the playlist...
            if (Radio.cachedInfos.Count == 0)
                return;

            // There could be a chance that the music has fully stopped without any user interaction, but since we're on
            // a radio station, we should seek nothing; just drop.
            if (PlaybackTools.State == PlaybackState.Stopped)
                PlaybackPositioningTools.Drop();
            Radio.advance = true;
            Radio.playerThread.Start();
            SpinWait.SpinUntil(() => PlaybackTools.Playing || Radio.failedToPlay);
            Radio.failedToPlay = false;
        }

        internal static void Pause()
        {
            Radio.advance = false;
            Radio.paused = true;
            PlaybackTools.Pause();
        }

        internal static void Stop(bool resetCurrentStation = true)
        {
            Radio.advance = false;
            Radio.paused = false;
            if (resetCurrentStation)
                Radio.currentStation = 1;
            PlaybackTools.Stop();
        }

        internal static void NextStation()
        {
            // In case we have no stations in the playlist...
            if (Radio.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop();
            Radio.currentStation++;
            if (Radio.currentStation > Radio.cachedInfos.Count)
                Radio.currentStation = 1;
        }

        internal static void PreviousStation()
        {
            // In case we have no stations in the playlist...
            if (Radio.cachedInfos.Count == 0)
                return;

            PlaybackTools.Stop();
            Radio.currentStation--;
            if (Radio.currentStation <= 0)
                Radio.currentStation = Radio.cachedInfos.Count;
        }

        internal static void PromptForAddStation()
        {
            string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the radio station. The URL to the station must provide an MPEG radio station. AAC ones are not supported yet.");
            Radio.populate = true;
            PopulateRadioStationInfo(path);
            Radio.populate = true;
            PopulateRadioStationInfo(Radio.cachedInfos[Radio.currentStation - 1].MusicPath);
        }

        internal static void Exit()
        {
            Radio.exiting = true;
            Radio.advance = false;
            PlaybackTools.Stop();
        }

        internal static void PopulateRadioStationInfo(string musicPath)
        {
            // Try to open the file after loading the library
            if (PlaybackTools.Playing || !Radio.populate)
                return;
            Radio.populate = false;
            if (Radio.cachedInfos.Any((csi) => csi.MusicPath == musicPath))
            {
                var instance = Radio.cachedInfos.Single((csi) => csi.MusicPath == musicPath);
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
                Radio.cachedInfos.Add(instance);
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
            if (Radio.cachedInfos.Count == 0)
                return;

            Radio.cachedInfos.RemoveAt(Radio.currentStation - 1);
            if (Radio.cachedInfos.Count > 0)
            {
                Radio.currentStation--;
                if (Radio.currentStation == 0)
                    Radio.currentStation = 1;
                Radio.populate = true;
                PopulateRadioStationInfo(Radio.cachedInfos[Radio.currentStation - 1].MusicPath);
            }
        }

        internal static void RemoveAllStations()
        {
            // In case we have no stations in the playlist...
            if (Radio.cachedInfos.Count == 0)
                return;

            for (int i = Radio.cachedInfos.Count; i > 0; i--)
                RemoveCurrentStation();
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
                [I]                 Radio station info
                [A]                 Add a radio station
                [B]                 Previous radio station
                [N]                 Next radio station
                [R]                 Remove current radio station
                [CTRL] + [R]        Remove all radio stations
                [E]                 Opens the equalizer
                [D] (when playing)  Device and driver info
                [Z]                 System info
                """
            );
        }

        internal static void ShowStationInfo()
        {
            InfoBoxColor.WriteInfoBox(
                $$"""
                Station info
                =========

                Radio station URL: {{Radio.cachedInfos[Radio.currentStation - 1].MusicPath}}
                Radio station name: {{Radio.cachedInfos[Radio.currentStation - 1].StationName}}
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

                Accurate rendering: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_ACCURATE)}}
                Buffer fill: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_BUFFERFILL)}}
                Decoding delay: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_DEC_DELAY)}}
                Encoding delay: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_ENC_DELAY)}}
                Encoding padding: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_ENC_PADDING)}}
                Frankenstein stream: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_FRANKENSTEIN)}}
                Fresh decoder: {{PlaybackTools.GetNativeState(mpg123_state.MPG123_FRESH_DECODER)}}
                """
            );
        }

        internal static void ShowDeviceDriver()
        {
            var builder = new StringBuilder();
            var currentTuple = DeviceTools.GetCurrent();
            var currentCachedTuple = DeviceTools.GetCurrentCached();
            var drivers = DeviceTools.GetDrivers();
            string activeDevice = "";
            foreach (var driver in drivers)
            {
                try
                {
                    builder.AppendLine($"- {driver.Key}: {driver.Value}");
                    var devices = DeviceTools.GetDevices(driver.Key, ref activeDevice);
                    foreach (var device in devices)
                        builder.AppendLine($"  - {device.Key}: {device.Value}");
                }
                catch
                {
                    continue;
                }
            }
            InfoBoxColor.WriteInfoBox(
                $$"""
                Device and Driver
                =================

                Device: {{currentTuple.device}}
                Driver: {{currentTuple.driver}}
                Device (cached): {{currentCachedTuple.device}}
                Driver (cached): {{currentCachedTuple.driver}}

                Available devices and drivers
                =============================

                {{builder}}
                """
            );
        }

        internal static void ShowSpecs()
        {
            InfoBoxColor.WriteInfoBox(
                $$"""
                BassBoom specifications
                =======================

                Basolia version: {{InitBasolia.BasoliaVersion}}
                MPG123 version: {{InitBasolia.MpgLibVersion}}
                OUT123 version: {{InitBasolia.OutLibVersion}}

                System specifications
                =====================

                System: {{(PlatformHelper.IsOnWindows() ? "Windows" : PlatformHelper.IsOnMacOS() ? "macOS" : "Unix/Linux")}}
                System Architecture: {{RuntimeInformation.OSArchitecture}}
                Process Architecture: {{RuntimeInformation.ProcessArchitecture}}
                System description: {{RuntimeInformation.OSDescription}}
                .NET description: {{RuntimeInformation.FrameworkDescription}}
                """
            );
        }
    }
}
