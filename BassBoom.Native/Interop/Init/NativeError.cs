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

namespace BassBoom.Native.Interop.Init
{
    internal enum mpg123_errors
    {
        MPG123_DONE = -12,
        MPG123_NEW_FORMAT = -11,
        MPG123_NEED_MORE = -10,
        MPG123_ERR = -1,
        MPG123_OK = 0,
        MPG123_BAD_OUTFORMAT,
        MPG123_BAD_CHANNEL,
        MPG123_BAD_RATE,
        MPG123_ERR_16TO8TABLE,
        MPG123_BAD_PARAM,
        MPG123_BAD_BUFFER,
        MPG123_OUT_OF_MEM,
        MPG123_NOT_INITIALIZED,
        MPG123_BAD_DECODER,
        MPG123_BAD_HANDLE,
        MPG123_NO_BUFFERS,
        MPG123_BAD_RVA,
        MPG123_NO_GAPLESS,
        MPG123_NO_SPACE,
        MPG123_BAD_TYPES,
        MPG123_BAD_BAND,
        MPG123_ERR_NULL,
        MPG123_ERR_READER,
        MPG123_NO_SEEK_FROM_END,
        MPG123_BAD_WHENCE,
        MPG123_NO_TIMEOUT,
        MPG123_BAD_FILE,
        MPG123_NO_SEEK,
        MPG123_NO_READER,
        MPG123_BAD_PARS,
        MPG123_BAD_INDEX_PAR,
        MPG123_OUT_OF_SYNC,
        MPG123_RESYNC_FAIL,
        MPG123_NO_8BIT,
        MPG123_BAD_ALIGN,
        MPG123_NULL_BUFFER,
        MPG123_NO_RELSEEK,
        MPG123_NULL_POINTER,
        MPG123_BAD_KEY,
        MPG123_NO_INDEX,
        MPG123_INDEX_FAIL,
        MPG123_BAD_DECODER_SETUP,
        MPG123_MISSING_FEATURE,
        MPG123_BAD_VALUE,
        MPG123_LSEEK_FAILED,
        MPG123_BAD_CUSTOM_IO,
        MPG123_LFS_OVERFLOW,
        MPG123_INT_OVERFLOW,
        MPG123_BAD_FLOAT
    }

    /// <summary>
    /// Error group from mpg123
    /// </summary>
    internal static unsafe class NativeError
    {
        /// <summary>
        /// MPG123_EXPORT const char* mpg123_plain_strerror(int errcode);
        /// </summary>
        internal delegate nint mpg123_plain_strerror(int errcode);

        /// <summary>
        /// MPG123_EXPORT const char* mpg123_strerror(mpg123_handle *mh);
        /// </summary>
        internal delegate nint mpg123_strerror(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_errcode(mpg123_handle *mh);
        /// </summary>
        internal delegate int mpg123_errcode(mpg123_handle* mh);
    }
}
