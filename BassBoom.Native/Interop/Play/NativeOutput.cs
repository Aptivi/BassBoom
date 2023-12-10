﻿//
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
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Play
{
    public enum mpg123_channelcount
    {
        MPG123_MONO = 1,
        MPG123_STEREO = 2
    }

    /// <summary>
    /// Output group from mpg123
    /// </summary>
    public static unsafe class NativeOutput
    {
        /// <summary>
        /// MPG123_EXPORT void mpg123_rates(const long **list, size_t *number);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_rates(long[] list, int* number);

        /// <summary>
        /// MPG123_EXPORT void mpg123_encodings(const int **list, size_t *number);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_encodings(int[] list, int* number);

        /// <summary>
        /// MPG123_EXPORT int mpg123_encsize(int encoding);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_encsize(int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_format_none(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_format_none(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_format_all(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_format_all(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_format(mpg123_handle *mh
        /// , long rate, int channels, int encodings);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_format(mpg123_handle* mh, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_format2(mpg123_handle *mh
        /// , long rate, int channels, int encodings);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_format2(mpg123_handle* mh, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_format_support(mpg123_handle *mh
        /// , long rate, int encoding);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_format_support(mpg123_handle* mh, long rate, int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getformat(mpg123_handle *mh
        /// , long *rate, int *channels, int *encoding);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getformat(mpg123_handle* mh, out long rate, out int channels, out int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getformat2(mpg123_handle *mh
        /// , long *rate, int *channels, int *encoding, int clear_flag);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getformat2(mpg123_handle* mh, long* rate, int* channels, int* encoding, int clear_flag);
    }
}
