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

namespace BassBoom.Native.Interop.Output
{
    internal enum out123_parms
    {
        OUT123_FLAGS = 1,
        OUT123_PRELOAD,
        OUT123_GAIN,
        OUT123_VERBOSE,
        OUT123_DEVICEBUFFER,
        OUT123_PROPFLAGS,
        OUT123_NAME,
        OUT123_BINDIR,
        OUT123_ADD_FLAGS,
        OUT123_REMOVE_FLAGS,
    }

    internal enum out123_flags
    {
        OUT123_HEADPHONES       = 0x01,
        OUT123_INTERNAL_SPEAKER = 0x02,
        OUT123_LINE_OUT         = 0x04,
        OUT123_QUIET            = 0x08,
        OUT123_KEEP_PLAYING     = 0x10,
        OUT123_MUTE             = 0x20
    }

    internal enum out123_propflags
    {
        OUT123_PROP_LIVE = 0x01,
        OUT123_PROP_PERSISTENT = 0x02
    }

    internal enum out123_error
    {
        OUT123_ERR = -1,
        OUT123_OK  = 0,
        OUT123_DOOM,
        OUT123_BAD_DRIVER_NAME,
        OUT123_BAD_DRIVER,
        OUT123_NO_DRIVER,
        OUT123_NOT_LIVE,
        OUT123_DEV_PLAY,
        OUT123_DEV_OPEN,
        OUT123_BUFFER_ERROR,
        OUT123_MODULE_ERROR,
        OUT123_ARG_ERROR,
        OUT123_BAD_PARAM,
        OUT123_SET_RO_PARAM,
        OUT123_BAD_HANDLE,
        OUT123_NOT_SUPPORTED,
        OUT123_DEV_ENUMERATE,
        OUT123_ERRCOUNT
    }

    internal unsafe struct out123_handle
    { }

    /// <summary>
    /// Output group from out123
    /// </summary>
    internal static unsafe class NativeOutputLib
    {
        /// <summary>
        /// MPG123_EXPORT const char *out123_distversion(unsigned int *major, unsigned int *minor, unsigned int *patch);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern IntPtr out123_distversion(ref uint major, ref uint minor, ref uint patch);

        /// <summary>
        /// MPG123_EXPORT unsigned int out123_libversion(unsigned int *patch);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern uint out123_libversion(ref uint patch);

        /// <summary>
        /// MPG123_EXPORT out123_handle *out123_new(void);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern out123_handle* out123_new();

        /// <summary>
        /// MPG123_EXPORT void out123_del(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_del(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT void out123_free(void *ptr);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_free(IntPtr ptr);

        /// <summary>
        /// MPG123_EXPORT const char* out123_strerror(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern nint out123_strerror(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT int out123_errcode(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_errcode(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT const char* out123_plain_strerror(int errcode);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern nint out123_plain_strerror(int errcode);

        /// <summary>
        /// MPG123_EXPORT int out123_set_buffer(out123_handle *ao, size_t buffer_bytes);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_set_buffer(out123_handle* ao, int buffer_bytes);

        /// <summary>
        /// MPG123_EXPORT int out123_param( out123_handle *ao, enum out123_parms code
        /// ,                 long value, double fvalue, const char *svalue );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_param(out123_handle* ao, out123_parms code, long @value, double fvalue, string svalue);

        /// <summary>
        /// MPG123_EXPORT int out123_param2( out123_handle *ao, int code
        /// ,                 long value, double fvalue, const char *svalue );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_param(out123_handle* ao, int code, long @value, double fvalue, string svalue);

        /// <summary>
        /// MPG123_EXPORT
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_getparam(out123_handle* ao, out123_parms code, long* ret_value, double* ret_fvalue, char* ret_svalue);

        /// <summary>
        /// MPG123_EXPORT int out123_getparam2( out123_handle *ao, int code
        /// ,                    long *ret_value, double *ret_fvalue, char* *ret_svalue );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_getparam2(out123_handle* ao, int code, long* ret_value, double* ret_fvalue, char* ret_svalue);

        /// <summary>
        /// MPG123_EXPORT int out123_param_from(out123_handle *ao, out123_handle* from_ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_param_from(out123_handle* ao, out123_handle* from_ao);

        /// <summary>
        /// MPG123_EXPORT int out123_drivers(out123_handle *ao, char ***names, char ***descr);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_drivers(out123_handle* ao, ref nint names, ref nint descr);

        /// <summary>
        /// MPG123_EXPORT int out123_devices( out123_handle *ao, const char *driver
        /// ,   char ***names, char ***descr, char **active_driver );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_devices(out123_handle* ao,
            [MarshalAs(UnmanagedType.LPStr)] string driver, out nint names, out nint descr, ref nint active_driver);

        /// <summary>
        /// MPG123_EXPORT void out123_stringlists_free(char **name, char **descr, int count);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_stringlists_free(char** name, char** descr, int count);

        /// <summary>
        /// MPG123_EXPORT int out123_open(out123_handle *ao, const char* driver, const char* device);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_open(out123_handle* ao, string driver, string device);

        /// <summary>
        /// MPG123_EXPORT int out123_driver_info(out123_handle *ao, char **driver, char **device);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_driver_info(out123_handle* ao, ref IntPtr driver, ref IntPtr device);

        /// <summary>
        /// MPG123_EXPORT void out123_close(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_close(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT int out123_encodings(out123_handle *ao, long rate, int channels);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_encodings(out123_handle* ao, long rate, int channels);

        /// <summary>
        /// MPG123_EXPORT int out123_encsize(int encoding);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_encsize(int encoding);

        /// <summary>
        /// MPG123_EXPORT int out123_formats( out123_handle *ao, const long *rates, int ratecount
        /// , int minchannels, int maxchannels
        /// , struct mpg123_fmt **fmtlist );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_formats(out123_handle* ao, IntPtr rates, int ratecount, int minchannels, int maxchannels, ref IntPtr fmtlist);

        /// <summary>
        /// MPG123_EXPORT int out123_enc_list(int **enclist);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_enc_list(int** enclist);

        /// <summary>
        /// MPG123_EXPORT int out123_enc_byname(const char *name);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_enc_byname(string name);

        /// <summary>
        /// MPG123_EXPORT const char* out123_enc_name(int encoding);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern string out123_enc_name(int encoding);

        /// <summary>
        /// MPG123_EXPORT const char* out123_enc_longname(int encoding);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern string out123_enc_longname(int encoding);

        /// <summary>
        /// MPG123_EXPORT int out123_start( out123_handle *ao
        /// ,                 long rate, int channels, int encoding );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_start(out123_handle* ao, long rate, int channels, int encoding);

        /// <summary>
        /// MPG123_EXPORT void out123_pause(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_pause(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT void out123_continue(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_continue(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT void out123_stop(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_stop(out123_handle *ao);

        /// <summary>
        /// MPG123_EXPORT size_t out123_play( out123_handle *ao
        ///                   , void *buffer, size_t bytes );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_play(out123_handle* ao, IntPtr buffer, int bytes);

        /// <summary>
        /// MPG123_EXPORT void out123_drop(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_drop(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT void out123_drain(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void out123_drain(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT void out123_ndrain(out123_handle *ao, size_t bytes);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern void out123_ndrain(out123_handle* ao, int bytes);

        /// <summary>
        /// MPG123_EXPORT size_t out123_buffered(out123_handle *ao);
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_buffered(out123_handle* ao);

        /// <summary>
        /// MPG123_EXPORT int out123_getformat( out123_handle *ao
        /// ,   long *rate, int *channels, int *encoding, int *framesize );
        /// </summary>
        [DllImport(MpgNative.LibraryNameOut, CharSet = CharSet.Ansi)]
        internal static extern int out123_getformat(out123_handle* ao, long* rate, int* channels, int* encoding, out int framesize);
    }
}
