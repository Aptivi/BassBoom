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
using System.Runtime.InteropServices;

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
        /// MPG123_EXPORT int mpg123_replace_buffer(mpv_handle *mh
        /// ,   void *data, size_t size);
        /// </summary>
        internal delegate int mpg123_replace_buffer(mpv_handle* mh, IntPtr data, int size);

        /// <summary>
        /// MPG123_EXPORT size_t mpg123_outblock(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_outblock(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_replace_reader( mpv_handle *mh
        /// ,   mpg123_ssize_t (*r_read) (int, void *, size_t)
        /// ,   off_t (*r_lseek)(int, off_t, int)
        /// );
        /// </summary>
        internal delegate int mpg123_replace_reader(mpv_handle* mh, r_read r_read, r_lseek r_lseek);

        /// <summary>
        /// MPG123_EXPORT int mpg123_replace_reader_handle( mpv_handle *mh
        /// ,   mpg123_ssize_t (*r_read) (void *, void *, size_t)
        /// ,   off_t (*r_lseek)(void *, off_t, int)
        /// ,   void (*cleanup)(void*) );
        /// </summary>
        internal delegate int mpg123_replace_reader_handle(mpv_handle* mh, r_read2 r_read, r_lseek2 r_lseek, _cleanup cleanup);

        /// <summary>
        /// MPG123_EXPORT int mpg123_reader64( mpv_handle *mh, int (*r_read) (void *, void *, size_t, size_t *), int64_t (*r_lseek)(void *, int64_t, int), void (*cleanup)(void*) );
        /// </summary>
        internal delegate int mpg123_reader64(mpv_handle* mh, r_read3 r_read, r_lseek3 r_lseek, _cleanup cleanup);
    }
}
