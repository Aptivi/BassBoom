using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Play
{
    /// <summary>
    /// Positioning group from mpg123
    /// </summary>
    public static unsafe class NativePositioning
    {
        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tell(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_tell(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tell64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_tell64(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tellframe(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_tellframe(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tellframe64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_tellframe64(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tell_stream(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_tell_stream(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tell_stream64(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_tell_stream64(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_seek( mpg123_handle *mh
        /// ,   off_t sampleoff, int whence );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_seek(mpg123_handle* mh, int sampleoff, int whence);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_seek64( mpg123_handle *mh
        /// ,   int64_t sampleoff, int whence );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_seek64(mpg123_handle* mh, long sampleoff, int whence);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_feedseek( mpg123_handle *mh
        /// ,   off_t sampleoff, int whence, off_t *input_offset );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_feedseek(mpg123_handle* mh, int sampleoff, int whence, int* input_offset);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_feedseek64( mpg123_handle *mh
        /// ,   int64_t sampleoff, int whence, int64_t *input_offset );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_feedseek64(mpg123_handle* mh, long sampleoff, int whence, long* input_offset);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_seek_frame( mpg123_handle *mh
        /// ,   off_t frameoff, int whence );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_seek_frame(mpg123_handle* mh, int frameoff, int whence);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_seek_frame64( mpg123_handle *mh
        /// ,   int64_t frameoff, int whence );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_seek_frame64(mpg123_handle* mh, long frameoff, int whence);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_timeframe(mpg123_handle *mh, double sec);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_timeframe(mpg123_handle *mh, double sec);

        /// <summary>
        /// MPG123_EXPORT int mpg123_index( mpg123_handle *mh
        /// ,   off_t **offsets, off_t *step, size_t * fill);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_index(mpg123_handle* mh, int*[] offsets, int* step, int* fill);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_timeframe64(mpg123_handle *mh, double sec);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern long mpg123_timeframe64(mpg123_handle* mh, double sec);

        /// <summary>
        /// MPG123_EXPORT int mpg123_index64( mpg123_handle *mh
        /// ,   int64_t **offsets, int64_t *step, size_t *fill );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_index64(mpg123_handle* mh, long*[] offsets, long* step, int* fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_index( mpg123_handle *mh
        /// ,   off_t *offsets, off_t step, size_t fill );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_index(mpg123_handle* mh, int* offsets, int step, int fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_index64( mpg123_handle *mh
        /// ,   int64_t *offsets, int64_t step, size_t fill );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_set_index64(mpg123_handle* mh, long* offsets, long step, int fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_position( mpg123_handle *mh, off_t frame_offset, off_t buffered_bytes, off_t *current_frame, off_t *frames_left, double *current_seconds, double *seconds_left);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_position(mpg123_handle* mh, int frame_offset, int buffered_bytes, int* current_frame, int* frames_left, double* current_seconds, double* seconds_left);
    }
}
