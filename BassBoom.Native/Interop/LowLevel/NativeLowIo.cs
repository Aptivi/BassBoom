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

using System;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.LowLevel
{
    /// <summary>
    /// Low-level I/O group from mpg123
    /// </summary>
    internal static unsafe class NativeLowIo
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int r_read(int val1, IntPtr val2, int val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int r_read2(IntPtr val1, IntPtr val2, int val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int r_read3(IntPtr val1, IntPtr val2, int val3, IntPtr val4);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int r_lseek(int val1, int val2, int val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int r_lseek2(IntPtr val1, int val2, int val3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]     
        internal delegate int r_lseek3(IntPtr val1, long val2, int val3);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void _cleanup(IntPtr val1);

        /// <summary>
        /// MPG123_EXPORT int mpg123_replace_buffer(mpg123_handle *mh
        /// ,   void *data, size_t size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_replace_buffer(mpg123_handle* mh, IntPtr data, int size);

        /// <summary>
        /// MPG123_EXPORT size_t mpg123_outblock(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_outblock(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_replace_reader( mpg123_handle *mh
        /// ,   mpg123_ssize_t (*r_read) (int, void *, size_t)
        /// ,   off_t (*r_lseek)(int, off_t, int)
        /// );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_replace_reader(mpg123_handle* mh, r_read r_read, r_lseek r_lseek);

        /// <summary>
        /// MPG123_EXPORT int mpg123_replace_reader_handle( mpg123_handle *mh
        /// ,   mpg123_ssize_t (*r_read) (void *, void *, size_t)
        /// ,   off_t (*r_lseek)(void *, off_t, int)
        /// ,   void (*cleanup)(void*) );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_replace_reader_handle(mpg123_handle* mh, r_read2 r_read, r_lseek2 r_lseek, _cleanup cleanup);

        /// <summary>
        /// MPG123_EXPORT int mpg123_reader64( mpg123_handle *mh, int (*r_read) (void *, void *, size_t, size_t *), int64_t (*r_lseek)(void *, int64_t, int), void (*cleanup)(void*) );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_reader64(mpg123_handle* mh, r_read3 r_read, r_lseek3 r_lseek, _cleanup cleanup);
    }
}
