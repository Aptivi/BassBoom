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

using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Runtime;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BassBoom.Basolia.Devices;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Analysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BassBoom.Basolia.Enumerations;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
        internal static bool bufferPlaying = false;
        internal static bool holding = false;
        internal static string radioIcy = "";
        private static PlaybackState state = PlaybackState.Stopped;

        /// <summary>
        /// Checks to see whether the music is playing
        /// </summary>
        public static bool Playing =>
            state == PlaybackState.Playing;

        /// <summary>
        /// The current state of the playback
        /// </summary>
        public static PlaybackState State =>
            state;

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        public static string RadioIcy =>
            radioIcy;

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        public static string RadioNowPlaying
        {
            get
            {
                string icy = RadioIcy;
                if (icy.Length == 0 || !FileTools.IsRadioStation)
                    return "";
                icy = Regex.Match(icy, @"StreamTitle='((?:[^']|\\')*)'").Groups[1].Value.Trim().Replace("\\'", "'");
                return icy;
            }
        }

        /// <summary>
        /// Plays the currently open file (synchronous)
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaOutException"></exception>
        public static void Play()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't play a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                var outHandle = Mpg123Instance._out123Handle;

                // First, get formats and reset them
                var (rate, channels, encoding) = FormatTools.GetFormatInfo();
                int resetStatus = NativeOutput.mpg123_format_none(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't reset output encoding", (mpg123_errors)resetStatus);

                // Set the format
                int formatStatus = NativeOutput.mpg123_format(handle, rate, channels, encoding);
                if (formatStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set output encoding", (mpg123_errors)formatStatus);
                Debug.WriteLine($"Format {rate}, {channels}, {encoding}");

                // Try to open output to device
                int openStatus = NativeOutputLib.out123_open(outHandle, DeviceTools.activeDriver, DeviceTools.activeDevice);
                if (openStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't open output to device {DeviceTools.activeDevice} on driver {DeviceTools.activeDriver}", (out123_error)openStatus);

                // Start the output
                int startStatus = NativeOutputLib.out123_start(outHandle, rate, channels, encoding);
                if (startStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't start the output.", (out123_error)startStatus);

                // Now, buffer the entire music file and create an empty array based on its size
                var bufferSize = AudioInfoTools.GetBufferSize();
                Debug.WriteLine($"Buffer size is {bufferSize}");
                int err;
                state = PlaybackState.Playing;
                do
                {
                    int num = 0;
                    int audioBytes = 0;
                    byte[] audio = null;

                    // First, let Basolia "hold on" until hold is released
                    while (holding)
                        Thread.Sleep(1);

                    // Now, play the MPEG buffer to the device
                    bufferPlaying = true;
                    err = DecodeTools.DecodeFrame(ref num, ref audio, ref audioBytes);
                    PlayBuffer(audio);
                    bufferPlaying = false;

                    // Check to see if we need more (radio)
                    if (FileTools.IsRadioStation && err == (int)mpg123_errors.MPG123_NEED_MORE)
                    {
                        err = (int)mpg123_errors.MPG123_OK;
                        FeedRadio();
                    }
                } while (err == (int)mpg123_errors.MPG123_OK && Playing);
                if (Playing)
                    state = PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Plays the currently open file (asynchronous)
        /// </summary>
        public static async Task PlayAsync() =>
            await Task.Run(Play);

        /// <summary>
        /// Pauses the currently open file
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void Pause()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't pause a file that's not open", mpg123_errors.MPG123_BAD_FILE);
            state = PlaybackState.Paused;
        }

        /// <summary>
        /// Stops the playback
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void Stop()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't stop a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Stop the music and seek to the beginning
            state = PlaybackState.Stopped;
            if (!FileTools.IsRadioStation)
                PlaybackPositioningTools.SeekToTheBeginning();
        }

        /// <summary>
        /// Sets the volume of this application
        /// </summary>
        /// <param name="volume">Volume from 0.0 to 1.0, inclusive</param>
        /// <exception cref="BasoliaOutException"></exception>
        public static void SetVolume(double volume)
        {
            InitBasolia.CheckInited();

            // Check the volume
            if (volume < 0)
                volume = 0;
            if (volume > 1)
                volume = 1;

            // Try to set the volume
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeVolume.mpg123_volume(handle, volume);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't set volume to {volume}", (out123_error)status);
            }
        }

        /// <summary>
        /// Gets the volume information
        /// </summary>
        /// <returns>A base linear volume from 0.0 to 1.0, an actual linear volume from 0.0 to 1.0, and the RVA volume in decibels (dB)</returns>
        /// <exception cref="BasoliaOutException"></exception>
        public static (double baseLinear, double actualLinear, double decibelsRva) GetVolume()
        {
            InitBasolia.CheckInited();

            double baseLinearAddr = 0;
            double actualLinearAddr = 0;
            double decibelsRvaAddr = 0;

            // Try to get the volume
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeVolume.mpg123_getvolume(handle, ref baseLinearAddr, ref actualLinearAddr, ref decibelsRvaAddr);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't get volume (base, really, and decibels)", (out123_error)status);
            }

            // Get the volume information
            return (baseLinearAddr, actualLinearAddr, decibelsRvaAddr);
        }

        /// <summary>
        /// Sets the equalizer band to any value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetEqualizer(PlaybackChannels channels, int bandIdx, double value)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeVolume.mpg123_eq(handle, (mpg123_channels)channels, bandIdx, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set equalizer band {bandIdx + 1}/32 to {value} under {channels}", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Sets the equalizer bands to any value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdxStart">Band index from 0 to 31 (first band to start from)</param>
        /// <param name="bandIdxEnd">Band index from 0 to 31 (second band to end to)</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetEqualizerRange(PlaybackChannels channels, int bandIdxStart, int bandIdxEnd, double value)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeVolume.mpg123_eq_bands(handle, (int)channels, bandIdxStart, bandIdxEnd, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set equalizer bands {bandIdxStart + 1}/32 -> {bandIdxEnd + 1}/32 to {value} under {channels}", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the equalizer band value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <exception cref="BasoliaException"></exception>
        public static double GetEqualizer(PlaybackChannels channels, int bandIdx)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                double eq = NativeVolume.mpg123_geteq(handle, (mpg123_channels)channels, bandIdx);
                return eq;
            }
        }

        /// <summary>
        /// Resets the equalizer band to its natural value
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void ResetEqualizer()
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeVolume.mpg123_reset_eq(handle);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't reset equalizer bands to their initial values!", (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the native state
        /// </summary>
        /// <param name="state">A native state to get</param>
        /// <returns>A number that represents the value of this state</returns>
        /// <exception cref="BasoliaException"></exception>
        public static (long, double) GetNativeState(PlaybackStateType state)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                long stateInt = 0;
                double stateDouble = 0;
                var handle = Mpg123Instance._mpg123Handle;
                int status = NativeStatus.mpg123_getstate(handle, (mpg123_state)state, ref stateInt, ref stateDouble);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't get native state of {state}!", (mpg123_errors)status);
                return (stateInt, stateDouble);
            }
        }

        internal static void FeedRadio()
        {
            if (!FileTools.IsOpened || !FileTools.IsRadioStation)
                return;

            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the MP3 frame length first
                string metaIntStr = FileTools.CurrentFile.Headers.GetValues("icy-metaint").First();
                int metaInt = int.Parse(metaIntStr);

                // Now, get the MP3 frame
                byte[] buffer = new byte[metaInt];
                int numBytesRead = 0;
                int numBytesToRead = metaInt;
                do
                {
                    int n = FileTools.CurrentFile.Stream.Read(buffer, numBytesRead, 1);
                    numBytesRead += n;
                    numBytesToRead -= n;
                } while (numBytesToRead > 0);

                // Fetch the metadata.
                int lengthOfMetaData = FileTools.CurrentFile.Stream.ReadByte();
                int metaBytesToRead = lengthOfMetaData * 16;
                Debug.WriteLine($"Buffer: {lengthOfMetaData} [{metaBytesToRead}]");
                byte[] metadataBytes = new byte[metaBytesToRead];
                FileTools.CurrentFile.Stream.Read(metadataBytes, 0, metaBytesToRead);
                string icy = Encoding.UTF8.GetString(metadataBytes).Replace("\0", "").Trim();
                if (!string.IsNullOrEmpty(icy))
                    radioIcy = icy;
                Debug.WriteLine($"{radioIcy}");

                // Copy the data to MPG123
                IntPtr data = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, data, buffer.Length);
                int feedResult = NativeInput.mpg123_feed(handle, data, buffer.Length);
                if (feedResult != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't feed.", mpg123_errors.MPG123_ERR);
            }
        }

        internal static int PlayBuffer(byte[] buffer)
        {
            unsafe
            {
                var outHandle = Mpg123Instance._out123Handle;
                IntPtr bufferPtr = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * buffer.Length);
                Marshal.Copy(buffer, 0, bufferPtr, buffer.Length);
                int size = NativeOutputLib.out123_play(outHandle, bufferPtr, buffer.Length);
                Marshal.FreeHGlobal(bufferPtr);
                return size;
            }
        }
    }
}
