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

using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Native.Interop.Init;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Event
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventProperty
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        public MpvValueFormat format;
        public System.IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventStartFile
    {
        public long playlist_entry_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventEndFile
    {
        public MpvEofReason reason;
        public int error;
        public long playlist_entry_id;
        public long playlist_insert_id;
        public int playlist_insert_num_entries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventClientMessage
    {
        public int num_args;
        public System.IntPtr args;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventHook
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        public ulong id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventCommand
    {
        public MpvNode result;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEvent
    {
        public MpvEventId event_id;
        public int error;
        public ulong reply_userdata;
        public System.IntPtr data;
    }

    /// <summary>
    /// Event group from libmpv
    /// </summary>
    internal static unsafe class NativeEvent
    {
        public delegate void mpv_wakeup_callback(nint d);

        /// <summary>
        /// MPV_EXPORT const char *mpv_event_name(mpv_event_id event);
        /// </summary>
        internal delegate nint mpv_event_name(MpvEventId @event);

        /// <summary>
        /// MPV_EXPORT int mpv_event_to_node(mpv_node *dst, mpv_event *src);
        /// </summary>
        internal delegate int mpv_event_to_node(out MpvNode dst, MpvEvent src);

        /// <summary>
        /// MPV_EXPORT int mpv_request_event(mpv_handle *ctx, mpv_event_id event, int enable);
        /// </summary>
        internal delegate int mpv_request_event(MpvHandle* ctx, MpvEventId @event, int enable);

        /// <summary>
        /// MPV_EXPORT mpv_event *mpv_wait_event(mpv_handle *ctx, double timeout);
        /// </summary>
        internal delegate MpvEvent mpv_wait_event(MpvHandle* ctx, double timeout);

        /// <summary>
        /// MPV_EXPORT void mpv_wakeup(mpv_handle *ctx);
        /// </summary>
        internal delegate void mpv_wakeup(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT void mpv_set_wakeup_callback(mpv_handle *ctx, void (*cb)(void *d), void *d);
        /// </summary>
        internal delegate void mpv_set_wakeup_callback(MpvHandle* ctx, mpv_wakeup_callback cb, nint d);

        /// <summary>
        /// MPV_EXPORT void mpv_wait_async_requests(mpv_handle *ctx);
        /// </summary>
        internal delegate void mpv_wait_async_requests(MpvHandle* ctx);
    }
}
