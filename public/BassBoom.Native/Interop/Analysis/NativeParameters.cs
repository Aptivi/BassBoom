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

using BassBoom.Native.Interop.Enumerations;
using BassBoom.Native.Interop.Init;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Analysis
{
    /// <summary>
    /// Parameters group from libmpv
    /// </summary>
    internal static unsafe class NativeParameters
    {
        /// <summary>
        /// MPV_EXPORT int mpv_set_option(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_option(MpvHandle* ctx, string name, MpvValueFormat format, ref string data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_option_string(mpv_handle *ctx, const char *name, const char *data);
        /// </summary>
        internal delegate int mpv_set_option_string(MpvHandle* ctx, string name, string data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property(MpvHandle* ctx, string name, MpvValueFormat format, ref string data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, ref string data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property_int(MpvHandle* ctx, string name, MpvValueFormat format, ref long data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property_int_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, ref long data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property_double(MpvHandle* ctx, string name, MpvValueFormat format, ref double data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_set_property_double_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, ref double data);

        /// <summary>
        /// MPV_EXPORT int mpv_set_property_string(mpv_handle *ctx, const char *name, const char *data);
        /// </summary>
        internal delegate int mpv_set_property_string(MpvHandle* ctx, string name, string data);

        /// <summary>
        /// MPV_EXPORT int mpv_del_property(mpv_handle *ctx, const char *name);
        /// </summary>
        internal delegate int mpv_del_property(MpvHandle* ctx, string name);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property(MpvHandle* ctx, string name, MpvValueFormat format, out nint data);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, out nint data);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property_int(MpvHandle* ctx, string name, MpvValueFormat format, out long data);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property_int_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, out long data);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property(mpv_handle *ctx, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property_double(MpvHandle* ctx, string name, MpvValueFormat format, out double data);

        /// <summary>
        /// MPV_EXPORT int mpv_get_property_async(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format, void *data);
        /// </summary>
        internal delegate int mpv_get_property_double_async(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format, out double data);

        /// <summary>
        /// MPV_EXPORT char *mpv_get_property_string(mpv_handle *ctx, const char *name);
        /// </summary>
        internal delegate nint mpv_get_property_string(MpvHandle* ctx, string name);

        /// <summary>
        /// MPV_EXPORT char *mpv_get_property_osd_string(mpv_handle *ctx, const char *name);
        /// </summary>
        internal delegate nint mpv_get_property_osd_string(MpvHandle* ctx, string name);

        /// <summary>
        /// MPV_EXPORT int mpv_observe_property(mpv_handle *ctx, uint64_t reply_userdata, const char *name, mpv_format format);
        /// </summary>
        internal delegate int mpv_observe_property(MpvHandle* ctx, ulong reply_userdata, string name, MpvValueFormat format);

        /// <summary>
        /// MPV_EXPORT int mpv_unobserve_property(mpv_handle *mpv, uint64_t registered_reply_userdata);
        /// </summary>
        internal delegate int mpv_unobserve_property(MpvHandle* mpv, ulong registered_reply_userdata);
    }
}
