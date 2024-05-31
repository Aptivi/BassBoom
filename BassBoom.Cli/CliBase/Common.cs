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
using SpecProbe.Platform;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Terminaux.Base.Buffered;
using Terminaux.Inputs.Styles.Infobox;

namespace BassBoom.Cli.CliBase
{
    internal static class Common
    {
        internal static double volume = 1.0;
        internal static bool enableDisco = false;
        internal static int currentPos = 1;
        internal static bool exiting = false;
        internal static bool advance = false;
        internal static bool populate = true;
        internal static bool paused = false;
        internal static bool failedToPlay = false;
        internal static bool isRadioMode = false;
        internal static readonly List<CachedSongInfo> cachedInfos = [];

        internal static CachedSongInfo CurrentCachedInfo =>
            cachedInfos.Count > 0 ? cachedInfos[currentPos - 1] : null;

        internal static void RaiseVolume()
        {
            volume += 0.05;
            if (volume > 1)
                volume = 1;
            PlaybackTools.SetVolume(volume);
        }

        internal static void LowerVolume()
        {
            volume -= 0.05;
            if (volume < 0)
                volume = 0;
            PlaybackTools.SetVolume(volume);
        }

        internal static void Exit()
        {
            exiting = true;
            advance = false;
            if (FileTools.IsOpened)
                PlaybackTools.Stop();
        }

        internal static void Switch(string musicPath)
        {
            if (FileTools.IsOpened)
                FileTools.CloseFile();
            if (isRadioMode)
                FileTools.OpenUrl(musicPath);
            else
                FileTools.OpenFile(musicPath);
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

                Decoders
                ========

                Supported decoders:
                  - {{string.Join("\n  - ", DecodeTools.GetDecoders(true))}}

                All decoders:
                  - {{string.Join("\n  - ", DecodeTools.GetDecoders(false))}}

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
                [S] (when idle)     Add a music directory to the playlist
                [B]                 Previous song
                [N]                 Next song
                [R]                 Remove current song
                [CTRL] + [R]        Remove all songs
                [S] (when playing)  Selectively seek
                [F] (when playing)  Seek to previous lyric
                [G] (when playing)  Seek to next lyric
                [J] (when playing)  Seek to current lyric
                [K] (when playing)  Seek to which lyric
                [C]                 Set repeat checkpoint
                [SHIFT] + [C]       Seek to repeat checkpoint
                [E]                 Opens the equalizer
                [D] (when playing)  Device and driver info
                [Z]                 System info
                """
            );
        }

        internal static void ShowHelpRadio()
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
                [CTRL] + [I]        Radio station extended info
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

        internal static void HandleKeypressCommon(ConsoleKeyInfo keystroke, Screen playerScreen, bool radio)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.UpArrow:
                    RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    LowerVolume();
                    break;
                case ConsoleKey.H:
                    if (radio)
                        ShowHelpRadio();
                    else
                        ShowHelp();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    ShowSpecs();
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.L:
                    enableDisco = !enableDisco;
                    break;
                case ConsoleKey.Q:
                    Exit();
                    break;
            }
        }
    }
}
