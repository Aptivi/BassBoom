﻿
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

using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Analysis
{
    public enum mpg123_vbr
    {
        MPG123_CBR = 0,
        MPG123_VBR,
        MPG123_ABR
    }

    public enum mpg123_version
    {
        MPG123_1_0 = 0,
        MPG123_2_0,
        MPG123_2_5
    }

    public enum mpg123_mode
    {
        MPG123_M_STEREO = 0,
        MPG123_M_JOINT,
        MPG123_M_DUAL,
        MPG123_M_MONO
    }

    public enum mpg123_flags
    {
        MPG123_CRC = 0x1,
        MPG123_COPYRIGHT = 0x2,
        MPG123_PRIVATE = 0x4,
        MPG123_ORIGINAL = 0x8
    }

    public enum mpg123_state
    {
        MPG123_ACCURATE = 1,
        MPG123_BUFFERFILL,
        MPG123_FRANKENSTEIN,
        MPG123_FRESH_DECODER,
        MPG123_ENC_DELAY,
        MPG123_ENC_PADDING,
        MPG123_DEC_DELAY
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct mpg123_frameinfo
    {
        mpg123_version version;
        int layer;
        long rate;
        mpg123_mode mode;
        int mode_ext;
        int framesize;
        mpg123_flags flags;
        int emphasis;
        int bitrate;
        int abr_rate;
        mpg123_vbr vbr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct mpg123_frameinfo2
    {
        int version;
        int layer;
        long rate;
        int mode;
        int mode_ext;
        int framesize;
        int flags;
        int emphasis;
        int bitrate;
        int abr_rate;
        int vbr;
    }

    /// <summary>
    /// Status group from mpg123
    /// </summary>
    public static unsafe class NativeStatus
    {
        /// <summary>
        /// MPG123_EXPORT int mpg123_info(mpg123_handle *mh, struct mpg123_frameinfo *mi);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_info(mpg123_handle* mh, mpg123_frameinfo* mi);

        /// <summary>
        /// MPG123_EXPORT int mpg123_info2(mpg123_handle *mh, struct mpg123_frameinfo2 *mi);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_info2(mpg123_handle* mh, mpg123_frameinfo2* mi);

        /// <summary>
        /// MPG123_EXPORT size_t mpg123_safe_buffer(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_safe_buffer();

        /// <summary>
        /// MPG123_EXPORT int mpg123_scan(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_scan(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_framelength(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_framelength(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_length(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_length(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_filesize(mpg123_handle *mh, off_t size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_filesize(mpg123_handle* mh, int size);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_framelength64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_framelength64(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_length64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_length64(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_filesize64(mpg123_handle *mh, int64_t size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_filesize64(mpg123_handle* mh, long size);

        /// <summary>
        /// MPG123_EXPORT double mpg123_tpf(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern double mpg123_tpf(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_spf(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_spf(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT long mpg123_clip(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_clip(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getstate( mpg123_handle *mh
        /// ,   enum mpg123_state key, long *val, double *fval );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getstate(mpg123_handle* mh, mpg123_state key, long* val, double* fval);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getstate2( mpg123_handle *mh
        /// ,   int key, long *val, double *fval );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getstate2(mpg123_handle* mh, int key, long* val, double* fval);
    }
}
