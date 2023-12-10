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

using BassBoom.Native.Runtime;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Interop.Init;
using System;
using BassBoom.Native.Interop.Output;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Analysis;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Audio format tools
    /// </summary>
    public static class FormatTools
    {
        /// <summary>
        /// Gets the format information
        /// </summary>
        public static (long rate, int channels, int encoding) GetFormatInfo()
        {
            long fileRate;
            int fileChannel, fileEncoding;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                
                // Get the rate, the number of channels, and encoding
                int length = NativeOutput.mpg123_getformat(handle, out fileRate, out fileChannel, out fileEncoding);
                if (length != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't determine the format of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return (fileRate, fileChannel, fileEncoding);
        }

        /// <summary>
        /// Gets the supported formats
        /// </summary>
        public static FormatInfo[] GetFormats()
        {
            var formats = new List<FormatInfo>();

            // We're now entering the dangerous zone
            int getStatus;
            nint fmtlist = IntPtr.Zero;
            unsafe
            {
                var outHandle = Mpg123Instance._out123Handle;

                // Get the list of supported formats
                getStatus = NativeOutputLib.out123_formats(outHandle, IntPtr.Zero, 0, 0, 0, ref fmtlist);
                if (getStatus == (int)out123_error.OUT123_ERR)
                    throw new BasoliaOutException("Can't get format information", (out123_error)getStatus);
            }

            // Now, iterate through the list of supported formats
            for (int i = 0; i < getStatus; i++)
            {
                var fmtStruct = Marshal.PtrToStructure<mpg123_fmt>(fmtlist);
                long rate = fmtStruct.rate;
                int channels = fmtStruct.channels;
                int encoding = fmtStruct.encoding;
                if (rate >= 0 && channels >= 0 && encoding >= 0)
                {
                    var fmtInstance = new FormatInfo(rate, channels, encoding);
                    formats.Add(fmtInstance);
                }
            }

            // We're now entering the safe zone
            return formats.ToArray();
        }
    }
}
