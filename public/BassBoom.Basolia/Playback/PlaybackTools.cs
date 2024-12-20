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
using BassBoom.Basolia.Format;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Play;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Analysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BassBoom.Basolia.Enumerations;
using BassBoom.Native;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Devices;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
        /// <summary>
        /// Checks to see whether the music is playing
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static bool IsPlaying(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            return basolia.state == PlaybackState.Playing;
        }

        /// <summary>
        /// The current state of the playback
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static PlaybackState GetState(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            return basolia.state;
        }

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static string GetRadioIcy(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            return basolia.radioIcy;
        }

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static string GetRadioNowPlaying(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            string icy = GetRadioIcy(basolia);
            if (icy.Length == 0 || !FileTools.IsRadioStation(basolia))
                return "";
            icy = Regex.Match(icy, @"StreamTitle='(.+?(?=\';))'").Groups[1].Value.Trim().Replace("\\'", "'");
            return icy;
        }

        /// <summary>
        /// Plays the currently open file (synchronous)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaOutException"></exception>
        public static void Play(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't play a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Reset the format. Orders here matter.
                var (rate, channels, encoding) = FormatTools.GetFormatInfo(basolia);
                FormatTools.NoFormat(basolia);

                // Set the format
                FormatTools.UseFormat(basolia, rate, (ChannelCount)channels, encoding);
                Debug.WriteLine($"Format {rate}, {channels}, {encoding}");

                // Try to open output to device
                OpenOutput(basolia);

                // Start the output
                StartOutput(basolia, rate, (ChannelCount)channels, encoding);

                // Now, buffer the entire music file and create an empty array based on its size
                var bufferSize = AudioInfoTools.GetBufferSize(basolia);
                Debug.WriteLine($"Buffer size is {bufferSize}");
                int err;
                basolia.state = PlaybackState.Playing;
                do
                {
                    int num = 0;
                    int audioBytes = 0;
                    byte[]? audio = null;

                    // First, let Basolia "hold on" until hold is released
                    while (basolia.holding)
                        Thread.Sleep(1);

                    // Now, play the MPEG buffer to the device
                    basolia.bufferPlaying = true;
                    err = DecodeTools.DecodeFrame(basolia, ref num, ref audio, ref audioBytes);
                    PlayBuffer(basolia, audio);
                    basolia.bufferPlaying = false;

                    // Check to see if we need more (radio)
                    if (FileTools.IsRadioStation(basolia) && err == (int)mpg123_errors.MPG123_NEED_MORE)
                    {
                        err = (int)mpg123_errors.MPG123_OK;
                        FeedRadio(basolia);
                    }
                } while (err == (int)mpg123_errors.MPG123_OK && IsPlaying(basolia));
                if (IsPlaying(basolia) || basolia.state == PlaybackState.Stopping)
                    basolia.state = PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Plays the currently open file (asynchronous)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static async Task PlayAsync(BasoliaMedia? basolia) =>
            await Task.Run(() => Play(basolia));

        /// <summary>
        /// Pauses the currently open file
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void Pause(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't pause a file that's not open", mpg123_errors.MPG123_BAD_FILE);
            basolia.state = PlaybackState.Paused;
        }

        /// <summary>
        /// Stops the playback
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void Stop(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't stop a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Stop the music and seek to the beginning
            basolia.state = basolia.state == PlaybackState.Playing ? PlaybackState.Stopping : PlaybackState.Stopped;
            SpinWait.SpinUntil(() => basolia.state == PlaybackState.Stopped);
            if (!FileTools.IsRadioStation(basolia))
                PlaybackPositioningTools.SeekToTheBeginning(basolia);
        }

        /// <summary>
        /// Sets the volume of this application
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="volume">Volume from 0.0 to 1.0 (volume booster off) or 3.0 (volume booster on), inclusive</param>
        /// <param name="volBoost">Whether to allow volumes larger than 1.0 up to 3.0</param>
        /// <exception cref="BasoliaOutException"></exception>
        public static void SetVolume(BasoliaMedia? basolia, double volume, bool volBoost = false)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check the volume
            double maxVolume = volBoost ? 3 : 1;
            if (volume < 0)
                volume = 0;
            if (volume > maxVolume)
                volume = maxVolume;

            // Try to set the volume
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_volume>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_volume));
                int status = @delegate.Invoke(handle, volume);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't set volume to {volume}", (out123_error)status);
            }
        }

        /// <summary>
        /// Gets the volume information
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>A base linear volume from 0.0 to 1.0, an actual linear volume from 0.0 to 1.0, and the RVA volume in decibels (dB)</returns>
        /// <exception cref="BasoliaOutException"></exception>
        public static (double baseLinear, double actualLinear, double decibelsRva) GetVolume(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            double baseLinearAddr = 0;
            double actualLinearAddr = 0;
            double decibelsRvaAddr = 0;

            // Try to get the volume
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_getvolume>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_getvolume));
                int status = @delegate.Invoke(handle, ref baseLinearAddr, ref actualLinearAddr, ref decibelsRvaAddr);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't get volume (base, really, and decibels)", (out123_error)status);
            }

            // Get the volume information
            return (baseLinearAddr, actualLinearAddr, decibelsRvaAddr);
        }

        /// <summary>
        /// Sets the equalizer band to any value
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetEqualizer(BasoliaMedia? basolia, PlaybackChannels channels, int bandIdx, double value)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_eq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_eq));
                int status = @delegate.Invoke(handle, (mpg123_channels)channels, bandIdx, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set equalizer band {bandIdx + 1}/32 to {value} under {channels}", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Sets the equalizer bands to any value
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdxStart">Band index from 0 to 31 (first band to start from)</param>
        /// <param name="bandIdxEnd">Band index from 0 to 31 (second band to end to)</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetEqualizerRange(BasoliaMedia? basolia, PlaybackChannels channels, int bandIdxStart, int bandIdxEnd, double value)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_eq_bands>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_eq_bands));
                int status = @delegate.Invoke(handle, (int)channels, bandIdxStart, bandIdxEnd, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set equalizer bands {bandIdxStart + 1}/32 -> {bandIdxEnd + 1}/32 to {value} under {channels}", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the equalizer band value
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <exception cref="BasoliaException"></exception>
        public static double GetEqualizer(BasoliaMedia? basolia, PlaybackChannels channels, int bandIdx)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_geteq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_geteq));
                double eq = @delegate.Invoke(handle, (mpg123_channels)channels, bandIdx);
                return eq;
            }
        }

        /// <summary>
        /// Resets the equalizer band to its natural value
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void ResetEqualizer(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_reset_eq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_reset_eq));
                int status = @delegate.Invoke(handle);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't reset equalizer bands to their initial values!", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the native state
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="state">A native state to get</param>
        /// <returns>A number that represents the value of this state</returns>
        /// <exception cref="BasoliaException"></exception>
        public static (long, double) GetNativeState(BasoliaMedia? basolia, PlaybackStateType state)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                long stateInt = 0;
                double stateDouble = 0;
                var handle = basolia._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_getstate>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_getstate));
                int status = @delegate.Invoke(handle, (mpg123_state)state, ref stateInt, ref stateDouble);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't get native state of {state}!", (mpg123_errors)status);
                return (stateInt, stateDouble);
            }
        }

        /// <summary>
        /// Opens the output to the device. You can set your preferred driver and device using <see cref="DeviceTools.SetActiveDriver(BasoliaMedia?, string)"/> and <see cref="DeviceTools.SetActiveDevice(BasoliaMedia?, string, string)"/>.
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void OpenOutput(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            if (basolia.isOutputOpen)
                return;

            // Try to open output to device
            unsafe
            {
                var outHandle = basolia._out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_open>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_open));
                int openStatus = @delegate.Invoke(outHandle, basolia.activeDriver, basolia.activeDevice);
                if (openStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't open output to device {basolia.activeDevice} on driver {basolia.activeDriver}", (out123_error)openStatus);
                basolia.isOutputOpen = true;
            }
        }

        /// <summary>
        /// Opens the output to the device. You can set your preferred driver and device using <see cref="DeviceTools.SetActiveDriver(BasoliaMedia?, string)"/> and <see cref="DeviceTools.SetActiveDevice(BasoliaMedia?, string, string)"/>.
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="rate">Rate to open. It must be supported and must match what you've set in <see cref="FormatTools.UseFormat(BasoliaMedia?, long, ChannelCount, int)"/></param>
        /// <param name="channels">Channels to open. They must be supported and must match what you've set in <see cref="FormatTools.UseFormat(BasoliaMedia?, long, ChannelCount, int)"/></param>
        /// <param name="encoding">Encoding to open. It must be supported and must match what you've set in <see cref="FormatTools.UseFormat(BasoliaMedia?, long, ChannelCount, int)"/></param>
        /// <exception cref="BasoliaException"></exception>
        public static void StartOutput(BasoliaMedia? basolia, long rate, ChannelCount channels, int encoding)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Sanity checks
            bool supported = FormatTools.IsFormatSupported(basolia, rate, encoding, out _);
            if (!basolia.isOutputOpen)
                throw new BasoliaOutException("You need to open the output", out123_error.OUT123_NOT_LIVE);
            if (!supported)
                throw new BasoliaOutException($"Selected rate [{rate} hz] and encoding [{encoding}] is not supported", out123_error.OUT123_NOT_SUPPORTED);

            // Try to open output to device
            unsafe
            {
                var outHandle = basolia._out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_start>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_start));
                int startStatus = @delegate.Invoke(outHandle, rate, (int)channels, encoding);
                if (startStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't start the output.", (out123_error)startStatus);
            }
        }

        /// <summary>
        /// Closes the output to the device.
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void CloseOutput(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            if (!basolia.isOutputOpen)
                return;

            // Try to open output to device
            unsafe
            {
                var outHandle = basolia._out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_stop>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_stop));
                @delegate.Invoke(outHandle);
                var delegate2 = MpgNative.GetDelegate<NativeOutputLib.out123_close>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_close));
                delegate2.Invoke(outHandle);
                basolia.isOutputOpen = false;
            }
        }

        internal static void FeedRadio(BasoliaMedia? basolia)
        {
            if (!FileTools.IsOpened(basolia) || !FileTools.IsRadioStation(basolia))
                return;
            var currentRadio = FileTools.CurrentFile(basolia);
            if (currentRadio is null)
                return;
            if (currentRadio.Headers is null)
                return;
            if (currentRadio.Stream is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            unsafe
            {
                // Get the MP3 frame length first
                string metaIntStr = currentRadio.Headers.GetValues("icy-metaint").First();
                int metaInt = int.Parse(metaIntStr);

                // Now, get the MP3 frame
                byte[] buffer = new byte[metaInt];
                int numBytesRead = 0;
                int numBytesToRead = metaInt;
                do
                {
                    int n = currentRadio.Stream.Read(buffer, numBytesRead, 1);
                    numBytesRead += n;
                    numBytesToRead -= n;
                } while (numBytesToRead > 0);

                // Fetch the metadata.
                int lengthOfMetaData = currentRadio.Stream.ReadByte();
                int metaBytesToRead = lengthOfMetaData * 16;
                Debug.WriteLine($"Buffer: {lengthOfMetaData} [{metaBytesToRead}]");
                byte[] metadataBytes = new byte[metaBytesToRead];
                currentRadio.Stream.Read(metadataBytes, 0, metaBytesToRead);
                string icy = Encoding.UTF8.GetString(metadataBytes).Replace("\0", "").Trim();
                if (!string.IsNullOrEmpty(icy))
                    basolia.radioIcy = icy;
                Debug.WriteLine($"{basolia.radioIcy}");

                // Copy the data to MPG123
                CopyBuffer(basolia, buffer);
            }
        }

        internal static async Task FeedStream(BasoliaMedia? basolia)
        {
            if (!FileTools.IsOpened(basolia) || FileTools.IsRadioStation(basolia))
                return;
            var currentStream = FileTools.CurrentFile(basolia);
            if (currentStream is null)
                return;
            if (currentStream.Stream is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Now, get the MP3 frame
            byte[] buffer = new byte[currentStream.Stream.Length];
            await currentStream.Stream.ReadAsync(buffer, 0, (int)currentStream.Stream.Length);

            // Copy the data to MPG123
            CopyBuffer(basolia, buffer);
        }

        internal static void CopyBuffer(BasoliaMedia? basolia, byte[]? buffer)
        {
            if (buffer is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            unsafe
            {
                var handle = basolia._mpg123Handle;

                // Copy the data to MPG123
                IntPtr data = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, data, buffer.Length);
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_feed>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_feed));
                int feedResult = @delegate.Invoke(handle, data, buffer.Length);
                if (feedResult != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't feed.", mpg123_errors.MPG123_ERR);
            }
        }

        internal static int PlayBuffer(BasoliaMedia? basolia, byte[]? buffer)
        {
            if (buffer is null)
                return 0;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            unsafe
            {
                var outHandle = basolia._out123Handle;
                IntPtr bufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * buffer.Length);
                Marshal.Copy(buffer, 0, bufferPtr, buffer.Length);
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_play>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_play));
                int size = @delegate.Invoke(outHandle, bufferPtr, buffer.Length);
                Marshal.FreeHGlobal(bufferPtr);
                return size;
            }
        }
    }
}
