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

namespace BassBoom.Native.Interop.Play
{
    /// <summary>
    /// Input group from mpg123
    /// </summary>
    internal static unsafe class NativeInput
    {
        /// <summary>
        /// MPG123_EXPORT int mpg123_open_fixed(mpv_handle *mh, const char *path
        /// , int channels, int encoding);
        /// </summary>
        internal delegate int mpg123_open_fixed(mpv_handle* mh, string path, int channels, int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open(mpv_handle *mh, const char *path);
        /// </summary>
        internal delegate int mpg123_open(mpv_handle* mh, string path);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_fd(mpv_handle *mh, int fd);
        /// </summary>
        internal delegate int mpg123_open_fd(mpv_handle* mh, int fd);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_handle(mpv_handle *mh, void *iohandle);
        /// </summary>
        internal delegate int mpg123_open_handle(mpv_handle* mh, IntPtr iohandle);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_feed(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_open_feed(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_close(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_close(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_read(mpv_handle *mh
        /// , void *outmemory, size_t outmemsize, size_t *done);
        /// </summary>
        internal delegate int mpg123_read(mpv_handle* mh, IntPtr outmemory, int outmemsize, out int done);

        /// <summary>
        /// MPG123_EXPORT int mpg123_feed(mpv_handle *mh
        /// ,   const unsigned char *in, size_t size);
        /// </summary>
        internal delegate int mpg123_feed(mpv_handle* mh, IntPtr @in, int size);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode(mpv_handle *mh
        /// ,   const unsigned char *inmemory, size_t inmemsize
        /// ,   void *outmemory, size_t outmemsize, size_t *done);
        /// </summary>
        internal delegate int mpg123_decode(mpv_handle* mh, byte* inmemory, int inmemsize, IntPtr outmemory, int outmemsize, int* done);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode_frame(mpv_handle *mh
        /// ,   off_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        internal delegate int mpg123_decode_frame(mpv_handle* mh, ref IntPtr num, ref IntPtr audio, ref IntPtr bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_decode(mpv_handle *mh
        /// ,   off_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        internal delegate int mpg123_framebyframe_decode(mpv_handle* mh, IntPtr num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode_frame64(mpv_handle *mh
        /// ,   int64_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        internal delegate int mpg123_decode_frame64(mpv_handle *mh, long* num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_decode64(mpv_handle *mh
        /// ,   int64_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        internal delegate int mpg123_framebyframe_decode64(mpv_handle* mh, long* num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_next(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_framebyframe_next(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framedata(mpv_handle *mh
        /// ,   unsigned long *header, unsigned char **bodydata, size_t *bodybytes);
        /// </summary>
        internal delegate int mpg123_framedata(mpv_handle* mh, ulong* header, string[] bodydata, int* bodybytes);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_framepos(mpv_handle *mh);
        /// </summary>
        internal delegate IntPtr mpg123_framepos(mpv_handle *mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_framepos64(mpv_handle *mh);
        /// </summary>
        internal delegate long mpg123_framepos64(mpv_handle* mh);
    }
}
