
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Interop.LowLevel;
using BassBoom.Native.Runtime;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BassBoom.Basolia.Devices;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
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
                var formatInfo = FormatTools.GetFormatInfo();
                int resetStatus = NativeOutput.mpg123_format_none(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't reset output encoding", (mpg123_errors)resetStatus);

                // Set the format
                int formatStatus = NativeOutput.mpg123_format(handle, formatInfo.rate, formatInfo.channels, formatInfo.encoding);
                if (formatStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set output encoding", (mpg123_errors)formatStatus);
                Debug.WriteLine($"Format {formatInfo.rate}, {formatInfo.channels}, {formatInfo.encoding}");

                // Try to open output to device
                int openStatus = NativeOutputLib.out123_open(outHandle, DeviceTools.activeDriver, DeviceTools.activeDevice);
                if (openStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't open output to device {DeviceTools.activeDevice} on driver {DeviceTools.activeDriver}", (out123_error)openStatus);

                // Start the output
                int startStatus = NativeOutputLib.out123_start(outHandle, formatInfo.rate, formatInfo.channels, formatInfo.encoding);
                if (startStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't start the output.", (out123_error)startStatus);

                // Now, buffer the entire music file and create an empty array based on its size
                var frameSize = AudioInfoTools.GetFrameSize();
                var bufferSize = AudioInfoTools.GetBufferSize();
                Debug.WriteLine($"Buffer size is {bufferSize}");
                var buffer = stackalloc byte[bufferSize];
                var bufferPtr = new IntPtr(buffer);
                int done = NativePositioning.mpg123_tell(handle);
                int err = (int)mpg123_errors.MPG123_OK;
                int samples = 0;
                Debug.WriteLine($"mpg123_tell() returned {done}");
                state = PlaybackState.Playing;
                do
                {
                    int played;
                    err = NativeInput.mpg123_read(handle, bufferPtr, bufferSize, out done);
                    played = NativeOutputLib.out123_play(outHandle, bufferPtr, done);
                    if (played != done)
                    {
                        Debug.WriteLine("Short read encountered.");
                        Debug.WriteLine($"Played {played}, but done {done}");
                    }
                    Debug.WriteLine($"{played}, {done}, {err}");
                    samples += played / frameSize;
                    Debug.WriteLine($"S: {samples}");
                } while (done != 0 && err == (int)mpg123_errors.MPG123_OK && Playing);
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
    }
}
