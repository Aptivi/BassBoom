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

namespace BassBoom.Native.Interop.Play
{
    /// <summary>
    /// Positioning group from mpg123
    /// </summary>
    internal static unsafe class NativePositioning
    {
        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tell(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_tell(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tell64(mpv_handle *mh);
        /// </summary>
        internal delegate long mpg123_tell64(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tellframe(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_tellframe(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tellframe64(mpv_handle *mh);
        /// </summary>
        internal delegate long mpg123_tellframe64(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_tell_stream(mpv_handle *mh);
        /// </summary>
        internal delegate int mpg123_tell_stream(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_tell_stream64(mpv_handle *mh);
        /// </summary>
        internal delegate long mpg123_tell_stream64(mpv_handle* mh);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_seek( mpv_handle *mh
        /// ,   off_t sampleoff, int whence );
        /// </summary>
        internal delegate int mpg123_seek(mpv_handle* mh, int sampleoff, int whence);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_seek64( mpv_handle *mh
        /// ,   int64_t sampleoff, int whence );
        /// </summary>
        internal delegate long mpg123_seek64(mpv_handle* mh, long sampleoff, int whence);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_feedseek( mpv_handle *mh
        /// ,   off_t sampleoff, int whence, off_t *input_offset );
        /// </summary>
        internal delegate int mpg123_feedseek(mpv_handle* mh, int sampleoff, int whence, int* input_offset);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_feedseek64( mpv_handle *mh
        /// ,   int64_t sampleoff, int whence, int64_t *input_offset );
        /// </summary>
        internal delegate long mpg123_feedseek64(mpv_handle* mh, long sampleoff, int whence, long* input_offset);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_seek_frame( mpv_handle *mh
        /// ,   off_t frameoff, int whence );
        /// </summary>
        internal delegate int mpg123_seek_frame(mpv_handle* mh, int frameoff, int whence);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_seek_frame64( mpv_handle *mh
        /// ,   int64_t frameoff, int whence );
        /// </summary>
        internal delegate long mpg123_seek_frame64(mpv_handle* mh, long frameoff, int whence);

        /// <summary>
        /// MPG123_EXPORT off_t mpg123_timeframe(mpv_handle *mh, double sec);
        /// </summary>
        internal delegate int mpg123_timeframe(mpv_handle *mh, double sec);

        /// <summary>
        /// MPG123_EXPORT int mpg123_index( mpv_handle *mh
        /// ,   off_t **offsets, off_t *step, size_t * fill);
        /// </summary>
        internal delegate int mpg123_index(mpv_handle* mh, int*[] offsets, int* step, int* fill);

        /// <summary>
        /// MPG123_EXPORT int64_t mpg123_timeframe64(mpv_handle *mh, double sec);
        /// </summary>
        internal delegate long mpg123_timeframe64(mpv_handle* mh, double sec);

        /// <summary>
        /// MPG123_EXPORT int mpg123_index64( mpv_handle *mh
        /// ,   int64_t **offsets, int64_t *step, size_t *fill );
        /// </summary>
        internal delegate int mpg123_index64(mpv_handle* mh, long*[] offsets, long* step, int* fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_index( mpv_handle *mh
        /// ,   off_t *offsets, off_t step, size_t fill );
        /// </summary>
        internal delegate int mpg123_set_index(mpv_handle* mh, int* offsets, int step, int fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_set_index64( mpv_handle *mh
        /// ,   int64_t *offsets, int64_t step, size_t fill );
        /// </summary>
        internal delegate int mpg123_set_index64(mpv_handle* mh, long* offsets, long step, int fill);

        /// <summary>
        /// MPG123_EXPORT int mpg123_position( mpv_handle *mh, off_t frame_offset, off_t buffered_bytes, off_t *current_frame, off_t *frames_left, double *current_seconds, double *seconds_left);
        /// </summary>
        internal delegate int mpg123_position(mpv_handle* mh, int frame_offset, int buffered_bytes, int* current_frame, int* frames_left, double* current_seconds, double* seconds_left);
    }
}
