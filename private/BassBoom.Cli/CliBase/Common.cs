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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BassBoom.Basolia;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Media.Format;
using BassBoom.Cli.Languages;
using BassBoom.Cli.Tools;
using BassBoom.Native.Interop.Init;
using SpecProbe.Software.Platform;
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
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            double maxVolume = volBoost ? 3 : 1;
            volume += 0.05;
            if (volume > maxVolume)
                volume = maxVolume;
            BassBoomCli.basolia.SetVolume(volume, volBoost);
        }

        internal static void LowerVolume()
        {
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            volume -= 0.05;
            if (volume < 0)
                volume = 0;
            BassBoomCli.basolia.SetVolume(volume, volBoost);
        }

        internal static void Exit()
        {
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            exiting = true;
            advance = false;
            if (BassBoomCli.basolia.IsOpened())
                BassBoomCli.basolia.Stop();
        }

        internal static void Switch(string musicPath)
        {
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            if (BassBoomCli.basolia.IsOpened())
                BassBoomCli.basolia.CloseFile();
            if (isRadioMode)
                BassBoomCli.basolia.OpenUrl(musicPath);
            else
                BassBoomCli.basolia.OpenFile(musicPath);
        }

        internal static void ShowDeviceDriver()
        {
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            var builder = new StringBuilder();
            var currentBuilder = new StringBuilder();
            if (BassBoomCli.basolia.IsPlaying())
            {
                var (driver, device) = BassBoomCli.basolia.GetCurrent();
                var cached = BassBoomCli.basolia.GetCurrentCached();
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICE") + $" {device}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DRIVER") + $" {driver}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICECACHED") + $" {cached.device}");
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DRIVERCACHED") + $" {cached.driver}");
            }
            else
                currentBuilder.AppendLine(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_DEVICESQUERYNOTPLAYING"));
            var drivers = BassBoomCli.basolia.GetDrivers();
            string activeDevice = "";
            foreach (var driver in drivers)
            {
                try
                {
                    builder.AppendLine($"- {driver.Key}: {driver.Value}");
                    var devices = BassBoomCli.basolia.GetDevices(driver.Key, ref activeDevice);
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
            if (BassBoomCli.basolia is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
            var devSpecs = new StringBuilder();
            if (devMode)
            {
                devSpecs.AppendLine("\n\n" + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_EXTRASPECS") + "\n");

                // Get all encodings and add them to a separate builder
                var encodingsBuilder = new StringBuilder();
                int[] encodings = BassBoomCli.basolia.GetEncodings();
                foreach (int encoding in encodings)
                {
                    // Get the name and the description
                    string name = BassBoomCli.basolia.GetEncodingName(encoding);
                    string desc = BassBoomCli.basolia.GetEncodingDescription(encoding);
                    int size = BassBoomCli.basolia.GetEncodingSize(encoding);
                    int sampleSize = FormatTools.GetSampleSize(encoding);
                    int zeroSample = FormatTools.GetZeroSample(encoding, sampleSize, 0);

                    encodingsBuilder.AppendLine($"  - {name} [{encoding}, {size} B]: {desc}");
                    encodingsBuilder.AppendLine("    - " + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_PCMSAMPLESIZE") + $" {sampleSize}");
                    encodingsBuilder.AppendLine("    - " + LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ZEROSAMPLE") + $" {zeroSample}");
                }

                // Get all rates and add them to a separate builder
                var ratesBuilder = new StringBuilder();
                int[] rates = BassBoomCli.basolia.GetRates();
                foreach (int rate in rates)
                    ratesBuilder.AppendLine($"  - {rate} Hz");

                // For playing files (not radio stations), add even more values
                var playingBuilder = new StringBuilder();
                if (BassBoomCli.basolia.IsOpened() && !isRadioMode)
                {
                    int durationSamples = BassBoomCli.basolia.GetDuration(true);
                    int frameLength = BassBoomCli.basolia.GetFrameLength();
                    int samplesFrame = BassBoomCli.basolia.GetSamplesPerFrame();
                    double secondsFrame = BassBoomCli.basolia.GetSecondsPerFrame();
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
                    $"  - {string.Join("\n  - ", BassBoomCli.basolia.GetDecoders(true))}" + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ALLDECODERS") + "\n" +
                    $"  - {string.Join("\n  - ", BassBoomCli.basolia.GetDecoders(false))}" + "\n\n" +

                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ENCODINGSANDRATES") + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_ENCODINGS") + "\n" +
                    encodingsBuilder.ToString() + "\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_RATES") + "\n" +
                    ratesBuilder.ToString() + "\n" +

                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_BUFFERINFO") + "\n\n" +
                    LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_GENERICBUFFERSIZE") + $"{BassBoomCli.basolia.GetGenericBufferSize()}{playingBuilder}");
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
                    if (BassBoomCli.basolia is null)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_BASOLIAMEDIA"), mpg123_errors.MPG123_BAD_HANDLE);
                    if (keystroke.Modifiers == ConsoleModifiers.Control)
                    {
                        var drivers = BassBoomCli.basolia.GetDrivers().Select((kvp) => new InputChoiceInfo(kvp.Key, kvp.Value)).ToArray();
                        int driverIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(drivers, LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SELECTDRIVER"));
                        playerScreen.RequireRefresh();
                        if (driverIdx < 0)
                            return;
                        var driver = drivers[driverIdx];
                        string active = "";
                        var devices = BassBoomCli.basolia.GetDevices(driver.ChoiceName, ref active).Select((kvp) => new InputChoiceInfo(kvp.Key, kvp.Value)).ToArray();
                        int deviceIdx = InfoBoxSelectionColor.WriteInfoBoxSelection(devices, LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_SELECTDEVICE").FormatString(active));
                        playerScreen.RequireRefresh();
                        if (deviceIdx < 0)
                            return;
                        var device = devices[deviceIdx];
                        BassBoomCli.basolia.SetActiveDriver(driver.ChoiceName);
                        BassBoomCli.basolia.SetActiveDevice(driver.ChoiceName, device.ChoiceName);
                    }
                    else if (keystroke.Modifiers == ConsoleModifiers.Shift)
                        BassBoomCli.basolia.Reset();
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
