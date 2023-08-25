
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BassBoom.Basolia.Devices;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
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

                // First, get formats and set them
                var formatInfo = FormatTools.GetFormatInfo();
                NativeOutput.mpg123_format_none(handle);
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
                    throw new BasoliaOutException($"Can't start the output.", (out123_error)openStatus);

                // Get the output format to get the frame size
                int frameSize = 0;
                int getStatus = NativeOutputLib.out123_getformat(outHandle, null, null, null, out frameSize);
                if (getStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't get the output.", (out123_error)openStatus);
                Debug.WriteLine($"Got frame size {frameSize}");

                // Now, buffer the entire music file and create an empty array based on its size
                var bufferSize = NativeLowIo.mpg123_outblock(handle);
                Debug.WriteLine($"Buffer size is {bufferSize}");
                var buffer = stackalloc byte[bufferSize];
                var bufferPtr = new IntPtr(buffer);
                int done = 0;
                int err = (int)mpg123_errors.MPG123_OK;
                int samples = 0;
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
                } while (done != 0 && err == (int)mpg123_errors.MPG123_OK);
            }
        }
    }
}
