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
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Runtime;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Decoder tools
    /// </summary>
    public static class DecodeTools
    {
        /// <summary>
        /// Decodes next MPEG frame to internal buffer or reads a frame and returns after setting a new format.
        /// </summary>
        /// <param name="num">Frame offset</param>
        /// <param name="audio">Array of decoded audio bytes</param>
        /// <param name="bytes">Number of bytes to read</param>
        /// <returns>MPG123_OK on success.</returns>
        public static int DecodeFrame(ref int num, ref byte[] audio, ref int bytes)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't decode the frame of a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the frame
                IntPtr numPtr, bytesPtr, audioPtr = IntPtr.Zero;
                numPtr = new IntPtr(num);
                bytesPtr = new IntPtr(bytes);
                int decodeStatus = NativeInput.mpg123_decode_frame(handle, ref numPtr, ref audioPtr, ref bytesPtr);
                num = numPtr.ToInt32();
                bytes = bytesPtr.ToInt32();
                audio = new byte[bytes];
                if (audioPtr != IntPtr.Zero)
                    Marshal.Copy(audioPtr, audio, 0, bytes);
                if (decodeStatus != (int)mpg123_errors.MPG123_OK &&
                    decodeStatus != (int)mpg123_errors.MPG123_NEW_FORMAT &&
                    decodeStatus != (int)mpg123_errors.MPG123_NEED_MORE &&
                    decodeStatus != (int)mpg123_errors.MPG123_DONE)
                    throw new BasoliaException("Can't decode frame", (mpg123_errors)decodeStatus);

                return decodeStatus;
            }

        }
    }
}
