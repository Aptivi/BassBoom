//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
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

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
        internal static bool bufferPlaying = false;
        internal static bool holding = false;
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
                } while (err == (int)mpg123_errors.MPG123_OK && Playing);
                if (Playing)
                    state = PlaybackState.Stopped;
            }
        }

        public static async Task PlayAsync() =>
            await Task.Run(Play);

        public static void Pause()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't pause a file that's not open", mpg123_errors.MPG123_BAD_FILE);
            state = PlaybackState.Paused;
        }

        public static void Stop()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't stop a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Stop the music and seek to the beginning
            state = PlaybackState.Stopped;
            PlaybackPositioningTools.SeekToTheBeginning();
        }

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
