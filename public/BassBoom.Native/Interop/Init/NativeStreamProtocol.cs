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

using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Init
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvStreamCallbackInfo
    {
        public nint cookie;
        public mpv_stream_cb_read_fn read_fn;
        public mpv_stream_cb_seek_fn seek_fn;
        public mpv_stream_cb_size_fn size_fn;
        public mpv_stream_cb_close_fn close_fn;
        public mpv_stream_cb_cancel_fn cancel_fn;
    }

    internal delegate int mpv_stream_cb_read_fn(nint cookie, nint buf, uint nbytes);
    internal delegate int mpv_stream_cb_seek_fn(nint cookie, int offset);
    internal delegate int mpv_stream_cb_size_fn(nint cookie);
    internal delegate void mpv_stream_cb_close_fn(nint cookie);
    internal delegate void mpv_stream_cb_cancel_fn(nint cookie);
    internal delegate int mpv_stream_cb_open_ro_fn(nint user_data, nint uri, ref MpvStreamCallbackInfo info);

    /// <summary>
    /// Custom stream protocol group from libmpv
    /// </summary>
    internal static unsafe class NativeStreamProtocol
    {
        /// <summary>
        /// MPV_EXPORT int mpv_stream_cb_add_ro(mpv_handle *ctx, const char *protocol, void *user_data, mpv_stream_cb_open_ro_fn open_fn);
        /// </summary>
        internal delegate int mpv_stream_cb_add_ro(MpvHandle* ctx, string protocol, nint user_data, mpv_stream_cb_open_ro_fn open_fn);
    }
}
