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

using BassBoom.Basolia;
using BassBoom.Basolia.Devices;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Tools;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Terminaux.Base.Buffered;
using Terminaux.Inputs.Styles;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.CyclicWriters.Renderer.Tools;
using Terminaux.Writer.MiscWriters;
using Textify.General;

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
        internal static bool volBoost = false;
        internal static readonly List<CachedSongInfo> cachedInfos = [];

        internal static CachedSongInfo? CurrentCachedInfo =>
            cachedInfos.Count > 0 ? cachedInfos[currentPos - 1] : null;

        internal static void RaiseVolume()
        {
            double maxVolume = volBoost ? 3 : 1;
            volume += 0.05;
            if (volume > maxVolume)
                volume = maxVolume;
            PlaybackTools.SetVolume(BassBoomCli.basolia, volume, volBoost);
        }

        internal static void LowerVolume()
        {
            volume -= 0.05;
            if (volume < 0)
                volume = 0;
            PlaybackTools.SetVolume(BassBoomCli.basolia, volume, volBoost);
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
                currentBuilder.AppendLine("Device:" + $" {device}");
                currentBuilder.AppendLine("Driver:" + $" {driver}");
                currentBuilder.AppendLine("Device (cached):" + $" {cached.device}");
                currentBuilder.AppendLine("Driver (cached):" + $" {cached.driver}");
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
            InfoBoxModalColor.WriteInfoBoxModal(
                "Device and Driver" + "\n\n" +
                currentBuilder.ToString() + "\n\n" +
                "Available devices and drivers" + "\n\n" +
                builder.ToString()
            );
        }

        internal static void ShowSpecs(bool devMode = false)
        {
            var devSpecs = new StringBuilder();
            if (devMode)
            {
                devSpecs.AppendLine("\n\n" + "Extra specs (for developers)" + "\n");

                // Get all encodings and add them to a separate builder
                var encodingsBuilder = new StringBuilder();
                int[] encodings = FormatTools.GetEncodings();
                foreach (int encoding in encodings)
                {
                    // Get the name and the description
                    string name = FormatTools.GetEncodingName(encoding);
                    string desc = FormatTools.GetEncodingDescription(encoding);
                    int size = FormatTools.GetEncodingSize(encoding);
                    int sampleSize = FormatTools.GetSampleSize(encoding);
                    int zeroSample = FormatTools.GetZeroSample(encoding, sampleSize, 0);

                    encodingsBuilder.AppendLine($"  - {name} [{encoding}, {size} B]: {desc}");
                    encodingsBuilder.AppendLine("    - " + "PCM sample size:" + $" {sampleSize}");
                    encodingsBuilder.AppendLine("    - " + "Zero sample (offset 0):" + $" {zeroSample}");
                }

                // Get all rates and add them to a separate builder
                var ratesBuilder = new StringBuilder();
                int[] rates = FormatTools.GetRates();
                foreach (int rate in rates)
                    ratesBuilder.AppendLine($"  - {rate} hertz");

                // For playing files (not radio stations), add even more values
                var playingBuilder = new StringBuilder();
                if (FileTools.IsOpened(BassBoomCli.basolia) && !isRadioMode)
                {
                    int durationSamples = AudioInfoTools.GetDuration(BassBoomCli.basolia, true);
                    int frameLength = AudioInfoTools.GetFrameLength(BassBoomCli.basolia);
                    int samplesFrame = AudioInfoTools.GetSamplesPerFrame(BassBoomCli.basolia);
                    double secondsFrame = AudioInfoTools.GetSecondsPerFrame(BassBoomCli.basolia);
                    playingBuilder.Append(
                        "\n" +
                        "Duration in samples:" + $"{durationSamples}" + "\n" +
                        "Frame length:" + $"{frameLength}" + "\n" +
                        "Samples/frame:" + $"{samplesFrame}" + "\n" +
                        "Seconds/frame:" + $"{secondsFrame}");
                }

                // Now, grab the necessary values and add them, too.
                devSpecs.Append(
                    "Decoders" + "\n\n" +
                    "Supported decoders:" + "\n" +
                    $"  - {string.Join("\n  - ", DecodeTools.GetDecoders(true))}" + "\n\n" +
                    "All decoders:" + "\n" +
                    $"  - {string.Join("\n  - ", DecodeTools.GetDecoders(false))}" + "\n\n" +

                    "Encodings and Rates" + "\n\n" +
                    "Encodings:" + "\n" +
                    encodingsBuilder.ToString() + "\n" +
                    "Rates:" + "\n" +
                    ratesBuilder.ToString() + "\n" +

                    "Buffer info" + "\n\n" +
                    "Generic buffer size:" + $"{AudioInfoTools.GetGenericBufferSize()}{playingBuilder}");
            }

            InfoBoxModalColor.WriteInfoBoxModal(
                "BassBoom specifications" + "\n\n" +
                "Basolia version:" + $" {InitBasolia.BasoliaVersion}" + "\n" +
                "MPG123 version:" + $" {InitBasolia.MpgLibVersion}" + "\n" +
                "OUT123 version:" + $" {InitBasolia.OutLibVersion}" + "\n\n" +

                "System specifications" + "\n\n" +
                "System:" + $" {(PlatformHelper.IsOnWindows() ? "Windows" : PlatformHelper.IsOnMacOS() ? "macOS" : "Unix/Linux")}" + "\n" +
                "System Architecture:" + $" {RuntimeInformation.OSArchitecture}" + "\n" +
                "Process Architecture:" + $" {RuntimeInformation.ProcessArchitecture}" + "\n" +
                "System description:" + $" {RuntimeInformation.OSDescription}" + "\n" +
                ".NET description:" + $" {RuntimeInformation.FrameworkDescription}{devSpecs}"
            );
        }

        internal static void ShowHelp()
        {
            InfoBoxModalColor.WriteInfoBoxModal("Available keystrokes",
                KeybindingTools.RenderKeybindingHelpText(Player.allBindings)
            );
        }

        internal static void ShowHelpRadio()
        {
            InfoBoxModalColor.WriteInfoBoxModal("Available keystrokes",
                KeybindingTools.RenderKeybindingHelpText(Radio.allBindings)
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
                    ShowSpecs(keystroke.Modifiers == ConsoleModifiers.Shift);
                    redraw = true;
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.L:
                    enableDisco = !enableDisco;
                    break;
                case ConsoleKey.V:
                    volBoost = !volBoost;
                    if (!volBoost && volume > 1.0)
                        RaiseVolume();
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
                        int deviceIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(devices, "Select a device. Current driver is {0}. ESC to quit.".FormatString(active));
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
                case ConsoleKey.F1:
                    string path = InfoBoxInputColor.WriteInfoBoxInput("Enter a path to the playlist file that you would like to save");
                    playerScreen.RequireRefresh();
                    if (string.IsNullOrEmpty(path))
                    {
                        redraw = true;
                        playerScreen.RequireRefresh();
                        break;
                    }

                    // Check for extension
                    string extension = Path.GetExtension(path);
                    if (extension != ".m3u" && extension != ".m3u8")
                        path += ".m3u";
                    
                    // Get a list of paths and write the file
                    string[] paths = cachedInfos.Select((csi) => csi.MusicPath).ToArray();
                    File.WriteAllLines(path, paths);
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
