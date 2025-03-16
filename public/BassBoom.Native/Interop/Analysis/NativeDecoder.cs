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

using BassBoom.Native.Interop.Init;
using System;

namespace BassBoom.Native.Interop.Analysis
{
    /// <summary>
    /// Decoder group from mpg123
    /// </summary>
    internal static unsafe class NativeDecoder
    {
        /// <summary>
        /// MPG123_EXPORT const char **mpg123_decoders(void);
        /// </summary>
        internal delegate IntPtr mpg123_decoders();

        /// <summary>
        /// MPG123_EXPORT const char **mpg123_supported_decoders(void);
        /// </summary>
        internal delegate IntPtr mpg123_supported_decoders();

        /// <summary>
        /// MPG123_EXPORT int mpg123_decoder(mpv_handle *mh, const char* decoder_name);
        /// </summary>
        internal delegate int mpg123_decoder(MpvHandle* mh, string decoder_name);

        /// <summary>
        /// MPG123_EXPORT const char* mpg123_current_decoder(mpv_handle *mh);
        /// </summary>
        internal delegate IntPtr mpg123_current_decoder(MpvHandle* mh);
    }
}
