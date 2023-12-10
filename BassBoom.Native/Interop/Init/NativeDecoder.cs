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

using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Init
{
    /// <summary>
    /// Decoder group from mpg123
    /// </summary>
    public static unsafe class NativeDecoder
    {
        /// <summary>
        /// MPG123_EXPORT const char **mpg123_decoders(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string[] mpg123_decoders();

        /// <summary>
        /// MPG123_EXPORT const char **mpg123_supported_decoders(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string[] mpg123_supported_decoders();

        /// <summary>
        /// MPG123_EXPORT int mpg123_decoder(mpg123_handle *mh, const char* decoder_name);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_decoder(mpg123_handle* mh, string decoder_name);

        /// <summary>
        /// MPG123_EXPORT const char* mpg123_current_decoder(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string mpg123_current_decoder(mpg123_handle* mh);
    }
}
