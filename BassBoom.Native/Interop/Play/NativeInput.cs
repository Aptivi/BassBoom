using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Play
{
    /// <summary>
    /// Input group from mpg123
    /// </summary>
    public static unsafe class NativeInput
    {
        /// <summary>
        /// MPG123_EXPORT int mpg123_open_fixed(mpg123_handle *mh, const char *path
        /// , int channels, int encoding);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_open_fixed(mpg123_handle* mh, string path, int channels, int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open(mpg123_handle *mh, const char *path);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_open(mpg123_handle* mh, string path);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_fd(mpg123_handle *mh, int fd);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_open_fd(mpg123_handle* mh, int fd);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_handle(mpg123_handle *mh, void *iohandle);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_open_handle(mpg123_handle* mh, IntPtr iohandle);

        /// <summary>
        /// MPG123_EXPORT int mpg123_open_feed(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_open_feed(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_close(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_close(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_read(mpg123_handle *mh
        /// , void *outmemory, size_t outmemsize, size_t *done);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_read(mpg123_handle* mh, IntPtr outmemory, int outmemsize, out int done);

        /// <summary>
        /// MPG123_EXPORT int mpg123_feed(mpg123_handle *mh
        /// ,   const unsigned char *in, size_t size);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_feed(mpg123_handle* mh, byte* @in, int size);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode(mpg123_handle *mh
        /// ,   const unsigned char *inmemory, size_t inmemsize
        /// ,   void *outmemory, size_t outmemsize, size_t *done);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_decode(mpg123_handle* mh, byte* inmemory, int inmemsize, IntPtr outmemory, int outmemsize, int* done);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode_frame(mpg123_handle *mh
        /// ,   off_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_decode_frame(mpg123_handle* mh, IntPtr num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_decode(mpg123_handle *mh
        /// ,   off_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_framebyframe_decode(mpg123_handle* mh, IntPtr num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_decode_frame64(mpg123_handle *mh
        /// ,   int64_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_decode_frame64(mpg123_handle *mh, long* num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_decode64(mpg123_handle *mh
        /// ,   int64_t *num, unsigned char **audio, size_t *bytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_framebyframe_decode64(mpg123_handle* mh, long* num, string[] audio, int* bytes);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framebyframe_next(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_framebyframe_next(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_framedata(mpg123_handle *mh
        /// ,   unsigned long *header, unsigned char **bodydata, size_t *bodybytes);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_framedata(mpg123_handle* mh, ulong* header, string[] bodydata, int* bodybytes);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_framepos(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern IntPtr mpg123_framepos(mpg123_handle *mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_framepos64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_framepos64(mpg123_handle* mh);
    }
}
