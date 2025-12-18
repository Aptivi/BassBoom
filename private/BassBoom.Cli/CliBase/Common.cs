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
using BassBoom.Cli.Languages;
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
using Terminaux.Inputs.Styles.Infobox.Tools;
using Terminaux.Writer.CyclicWriters.Renderer.Tools;
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
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICE") + $" {device}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DRIVER") + $" {driver}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICECACHED") + $" {cached.device}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DRIVERCACHED") + $" {cached.driver}");
            }
            else
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICESQUERYNOTPLAYING"));
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
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICEANDDRIVER") + "\n\n" +
                currentBuilder.ToString() + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_AVAILABLEDEVICESANDDRIVERS") + "\n\n" +
                builder.ToString()
            );
        }

        internal static void ShowSpecs(bool devMode = false)
        {
            var devSpecs = new StringBuilder();
            if (devMode)
            {
                devSpecs.AppendLine("\n\n" + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_EXTRASPECS") + "\n");

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
                    encodingsBuilder.AppendLine("    - " + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_PCMSAMPLESIZE") + $" {sampleSize}");
                    encodingsBuilder.AppendLine("    - " + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ZEROSAMPLE") + $" {zeroSample}");
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
                        LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DURATIONSAMPLES") + $"{durationSamples}" + "\n" +
                        LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_FRAMELEN") + $"{frameLength}" + "\n" +
                        LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SPF") + $"{samplesFrame}" + "\n" +
                        LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_TPF") + $"{secondsFrame}");
                }

                // Now, grab the necessary values and add them, too.
                devSpecs.Append(
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DECODERS") + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SUPPORTEDDECODERS") + "\n" +
                    $"  - {string.Join("\n  - ", DecodeTools.GetDecoders(true))}" + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ALLDECODERS") + "\n" +
                    $"  - {string.Join("\n  - ", DecodeTools.GetDecoders(false))}" + "\n\n" +

                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ENCODINGSANDRATES") + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ENCODINGS") + "\n" +
                    encodingsBuilder.ToString() + "\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_RATES") + "\n" +
                    ratesBuilder.ToString() + "\n" +

                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BUFFERINFO") + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_GENERICBUFFERSIZE") + $"{AudioInfoTools.GetGenericBufferSize()}{playingBuilder}");
            }

            InfoBoxModalColor.WriteInfoBoxModal(
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BASSBOOMSPECS") + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BASSBOOMSPECS_BASOLIAVER") + $" {InitBasolia.BasoliaVersion}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BASSBOOMSPECS_MPG123VER") + $" {InitBasolia.MpgLibVersion}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BASSBOOMSPECS_OUT123VER") + $" {InitBasolia.OutLibVersion}" + "\n\n" +

                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS") + "\n\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS_SYSTEM") + $" {(PlatformHelper.IsOnWindows() ? "Windows" : PlatformHelper.IsOnMacOS() ? "macOS" : "Unix/Linux")}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS_SYSTEMARCH") + $" {RuntimeInformation.OSArchitecture}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS_PROCESSARCH") + $" {RuntimeInformation.ProcessArchitecture}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS_SYSTEMDESC") + $" {RuntimeInformation.OSDescription}" + "\n" +
                LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SYSTEMSPECS_DOTNETDESC") + $" {RuntimeInformation.FrameworkDescription}{devSpecs}"
            );
        }

        internal static void ShowHelp()
        {
            InfoBoxModalColor.WriteInfoBoxModal(KeybindingTools.RenderKeybindingHelpText(Player.AllBindings), new InfoBoxSettings()
            {
                Title = LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_AVAILABLEKEYSTROKES"),
            });
        }

        internal static void ShowHelpRadio()
        {
            InfoBoxModalColor.WriteInfoBoxModal(KeybindingTools.RenderKeybindingHelpText(Radio.AllBindings), new InfoBoxSettings()
            {
                Title = LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_AVAILABLEKEYSTROKES"),
            });
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
                    ShowSpecs(keystroke.Modifiers == ConsoleModifiers.Shift);
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
                        int driverIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(drivers, LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SELECTDRIVER"));
                        playerScreen.RequireRefresh();
                        if (driverIdx < 0)
                            return;
                        var driver = drivers[driverIdx];
                        string active = "";
                        var devices = DeviceTools.GetDevices(BassBoomCli.basolia, driver.ChoiceName, ref active).Select((kvp) => new InputChoiceInfo(kvp.Key, kvp.Value)).ToArray();
                        int deviceIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(devices, LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SELECTDEVICE").FormatString(active));
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
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.F1:
                    string path = InfoBoxInputColor.WriteInfoBoxInput(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_PATHTOPLAYLIST"));
                    playerScreen.RequireRefresh();
                    if (string.IsNullOrEmpty(path))
                    {
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
                    playerScreen.RequireRefresh();
                    break;
                case ConsoleKey.Q:
                    Exit();
                    break;
            }
        }
    }
}
