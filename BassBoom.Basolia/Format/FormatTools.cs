
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

using BassBoom.Native.Runtime;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BassBoom.Native.Interop.Init;
using BassBoom.Basolia.File;

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
    }
}
