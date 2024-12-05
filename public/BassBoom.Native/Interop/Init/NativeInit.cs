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

namespace BassBoom.Native.Interop.Init
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct mpg123_handle
    { }

    internal enum mpg123_parms
    {
        MPG123_VERBOSE = 0,
        MPG123_FLAGS,
        MPG123_ADD_FLAGS,
        MPG123_FORCE_RATE,
        MPG123_DOWN_SAMPLE,
        MPG123_RVA,
        MPG123_DOWNSPEED,
        MPG123_UPSPEED,
        MPG123_START_FRAME,
        MPG123_DECODE_FRAMES,
        MPG123_ICY_INTERVAL,
        MPG123_OUTSCALE,
        MPG123_TIMEOUT,
        MPG123_REMOVE_FLAGS,
        MPG123_RESYNC_LIMIT,
        MPG123_INDEX_SIZE,
        MPG123_PREFRAMES,
        MPG123_FEEDPOOL,
        MPG123_FEEDBUFFER,
        MPG123_FREEFORMAT_SIZE
    }

    internal enum mpg123_param_flags
    {
        MPG123_FORCE_MONO = 0x7,
        MPG123_MONO_LEFT = 0x1,
        MPG123_MONO_RIGHT = 0x2,
        MPG123_MONO_MIX = 0x4,
        MPG123_FORCE_STEREO = 0x8,
        MPG123_FORCE_8BIT = 0x10,
        MPG123_QUIET = 0x20,
        MPG123_GAPLESS = 0x40,
        MPG123_NO_RESYNC = 0x80,
        MPG123_SEEKBUFFER = 0x100,
        MPG123_FUZZY = 0x200,
        MPG123_FORCE_FLOAT = 0x400,
        MPG123_PLAIN_ID3TEXT = 0x800,
        MPG123_IGNORE_STREAMLENGTH = 0x1000,
        MPG123_SKIP_ID3V2 = 0x2000,
        MPG123_IGNORE_INFOFRAME = 0x4000,
        MPG123_AUTO_RESAMPLE = 0x8000,
        MPG123_PICTURE = 0x10000,
        MPG123_NO_PEEK_END = 0x20000,
        MPG123_FORCE_SEEKABLE = 0x40000,
        MPG123_STORE_RAW_ID3 = 0x80000,
        MPG123_FORCE_ENDIAN = 0x100000,
        MPG123_BIG_ENDIAN = 0x200000,
        MPG123_NO_READAHEAD = 0x400000,
        MPG123_FLOAT_FALLBACK = 0x800000,
        MPG123_NO_FRANKENSTEIN = 0x1000000
    }

    internal enum mpg123_param_rva
    {
        MPG123_RVA_OFF = 0,
        MPG123_RVA_MIX = 1,
        MPG123_RVA_ALBUM = 2,
        MPG123_RVA_MAX = MPG123_RVA_ALBUM
    }

    internal enum mpg123_feature_set
    {
        MPG123_FEATURE_ABI_UTF8OPEN = 0,
        MPG123_FEATURE_OUTPUT_8BIT,
        MPG123_FEATURE_OUTPUT_16BIT,
        MPG123_FEATURE_OUTPUT_32BIT,
        MPG123_FEATURE_INDEX,
        MPG123_FEATURE_PARSE_ID3V2,
        MPG123_FEATURE_DECODE_LAYER1,
        MPG123_FEATURE_DECODE_LAYER2,
        MPG123_FEATURE_DECODE_LAYER3,
        MPG123_FEATURE_DECODE_ACCURATE,
        MPG123_FEATURE_DECODE_DOWNSAMPLE,
        MPG123_FEATURE_DECODE_NTOM,
        MPG123_FEATURE_PARSE_ICY,
        MPG123_FEATURE_TIMEOUT_READ,
        MPG123_FEATURE_EQUALIZER,
        MPG123_FEATURE_MOREINFO,
        MPG123_FEATURE_OUTPUT_FLOAT32,
        MPG123_FEATURE_OUTPUT_FLOAT64
    }

    /// <summary>
    /// Init group from mpg123
    /// </summary>
    internal static unsafe class NativeInit
    {
        /// <summary>
        /// const char *mpg123_distversion(unsigned int *major, unsigned int *minor, unsigned int *patch)
        /// </summary>
        internal delegate IntPtr mpg123_distversion(ref uint major, ref uint minor, ref uint patch);

        /// <summary>
        /// unsigned int mpg123_libversion(unsigned int *patch);
        /// </summary>
        internal delegate uint mpg123_libversion(ref uint patch);

        /// <summary>
        /// MPG123_EXPORT int mpg123_init (void)
        /// </summary>
        internal delegate int mpg123_init();

        /// <summary>
        /// MPG123_EXPORT mpg123_handle* mpg123_new (const char* decoder, int* error)
        /// </summary>
        internal delegate mpg123_handle* mpg123_new([MarshalAs(UnmanagedType.LPStr)] string? decoder, int* error);

        /// <summary>
        /// MPG123_EXPORT void mpg123_delete(mpg123_handle* mh)
        /// </summary>
        internal delegate void mpg123_delete(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT void mpg123_free(void* ptr)
        /// </summary>
        internal delegate void mpg123_free(IntPtr ptr);

        /// <summary>
        /// MPG123_EXPORT int mpg123_param(mpg123_handle *mh
        /// , enum mpg123_parms type, long value, double fvalue);
        /// </summary>
        internal delegate int mpg123_param(mpg123_handle* mh, mpg123_parms type, long value, double fvalue);

        /// <summary>                                           
        /// MPG123_EXPORT int mpg123_param2(mpg123_handle *mh    
        /// , int type, long value, double fvalue);
        /// </summary>
        internal delegate int mpg123_param2(mpg123_handle* mh, int type, long value, double fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getparam(mpg123_handle *mh
        /// , enum mpg123_parms type, long *value, double *fvalue);
        /// </summary>
        internal delegate int mpg123_getparam(mpg123_handle* mh, mpg123_parms type, long* value, double* fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getparam2(mpg123_handle *mh
        /// , int type, long *value, double *fvalue);
        /// </summary>
        internal delegate int mpg123_getparam2(mpg123_handle* mh, int type, long* value, double* fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_feature(const enum mpg123_feature_set key);
        /// </summary>
        internal delegate int mpg123_feature(mpg123_feature_set key);

        /// <summary>
        /// MPG123_EXPORT int mpg123_feature2(int key);
        /// </summary>
        internal delegate int mpg123_feature2(int key);

        /// <summary>
        /// int setenv(const char *name, const char *value, int overwrite);
        /// </summary>
        [DllImport(MpgNative.LibcName, CharSet = CharSet.Ansi)]
        internal static extern int setenv(string name, string value, int overwrite);

        [DllImport("UCRTBASE.DLL")]
        internal static extern int _putenv_s(string e, string v);
    }
}
