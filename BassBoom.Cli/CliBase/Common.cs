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
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Terminaux.Base.Buffered;
using Terminaux.Inputs;
using Terminaux.Inputs.Styles;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.MiscWriters;

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
        internal static bool redraw = true;
        internal static readonly List<CachedSongInfo> cachedInfos = [];

        internal static CachedSongInfo? CurrentCachedInfo =>
            cachedInfos.Count > 0 ? cachedInfos[currentPos - 1] : null;

        internal static void RaiseVolume()
        {
            volume += 0.05;
            if (volume > 1)
                volume = 1;
            PlaybackTools.SetVolume(BassBoomCli.basolia, volume);
        }

        internal static void LowerVolume()
        {
            volume -= 0.05;
            if (volume < 0)
                volume = 0;
            PlaybackTools.SetVolume(BassBoomCli.basolia, volume);
        }

        internal static void Exit()
        {
            exiting = true;
            advance = false;
            if (FileTools.IsOpened(BassBoomCli.basolia))
                PlaybackTools.Stop(BassBoomCli.basolia);
        }

        internal static void Switch(string musicPath)
        {
            if (FileTools.IsOpened(BassBoomCli.basolia))
                FileTools.CloseFile(BassBoomCli.basolia);
            if (isRadioMode)
                FileTools.OpenUrl(BassBoomCli.basolia, musicPath);
            else
                FileTools.OpenFile(BassBoomCli.basolia, musicPath);
        }

        internal static void ShowDeviceDriver()
        {
            var builder = new StringBuilder();
            var currentBuilder = new StringBuilder();
            if (PlaybackTools.IsPlaying(BassBoomCli.basolia))
            {
                var (driver, device) = DeviceTools.GetCurrent(BassBoomCli.basolia);
                var cached = DeviceTools.GetCurrentCached(BassBoomCli.basolia);
                currentBuilder.AppendLine(
                    $$"""
                    Device: {{device}}
                    Driver: {{driver}}
                    Device (cached): {{cached.device}}
                    Driver (cached): {{cached.driver}}
                    """
                );
            }
            else
                currentBuilder.AppendLine("Can't query current devices while not playing.");
            var drivers = DeviceTools.GetDrivers(BassBoomCli.basolia);
            string activeDevice = "";
            foreach (var driver in drivers)
            {
                try
                {
                    builder.AppendLine($"- {driver.Key}: {driver.Value}");
                    var devices = DeviceTools.GetDevices(BassBoomCli.basolia, driver.Key, ref activeDevice);
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

                {{currentBuilder}}

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
            InfoBoxColor.WriteInfoBox("Available keystrokes",
                KeybindingsWriter.RenderKeybindingHelpText(Player.allBindings)
            );
        }

        internal static void ShowHelpRadio()
        {
            InfoBoxColor.WriteInfoBox("Available keystrokes",
                KeybindingsWriter.RenderKeybindingHelpText(Radio.allBindings)
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
                    redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.E:
                    Equalizer.OpenEqualizer(playerScreen);
                    redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Z:
                    ShowSpecs();
                    redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.L:
                    enableDisco = !enableDisco;
                    break;
                case ConsoleKey.D:
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                    {
                        var drivers = DeviceTools.GetDrivers(BassBoomCli.basolia).Select((kvp) => new InputChoiceInfo(kvp.Key, kvp.Value)).ToArray();
                        int driverIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(drivers, "Select a driver. ESC to quit.");
                        playerScreen.RequireRefresh();
                        if (driverIdx < 0)
                            return;
                        var driver = drivers[driverIdx];
                        string active = "";
                        var devices = DeviceTools.GetDevices(BassBoomCli.basolia, driver.ChoiceName, ref active).Select((kvp) => new InputChoiceInfo(kvp.Key, kvp.Value)).ToArray();
                        int deviceIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(devices, $"Select a device. Current driver is {active}. ESC to quit.");
                        playerScreen.RequireRefresh();
                        if (deviceIdx < 0)
                            return;
                        var device = devices[deviceIdx];
                        DeviceTools.SetActiveDriver(BassBoomCli.basolia, driver.ChoiceName);
                        DeviceTools.SetActiveDevice(BassBoomCli.basolia, driver.ChoiceName, device.ChoiceName);
                    }
                    else if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        DeviceTools.Reset(BassBoomCli.basolia);
                    else
                        ShowDeviceDriver();
                    redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Q:
                    Exit();
                    break;
            }
        }
    }
}
