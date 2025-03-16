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

namespace BassBoom.Native.Interop.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvRenderContext
    { }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvRenderParam
    {
        public MpvRenderParamType type;
        public nint data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvRenderFrameInfo
    {
        public ulong flags;
        public long target_time;
    }

    /// <summary>
    /// Video rendering group from libmpv
    /// </summary>
    internal static unsafe class NativeRender
    {
        public delegate void mpv_render_update_fn(nint cb_ctx);

        /// <summary>
        /// MPV_EXPORT int mpv_render_context_create(mpv_render_context **res, mpv_handle *mpv, mpv_render_param *params);
        /// </summary>
        internal delegate int mpv_render_context_create(out MpvRenderContext* res, MpvHandle* mpv, nint parameters);

        /// <summary>
        /// MPV_EXPORT int mpv_render_context_set_parameter(mpv_render_context *ctx, mpv_render_param param);
        /// </summary>
        internal delegate int mpv_render_context_set_parameter(MpvRenderContext* ctx, MpvRenderParam param);

        /// <summary>
        /// MPV_EXPORT int mpv_render_context_get_info(mpv_render_context *ctx, mpv_render_param param);
        /// </summary>
        internal delegate int mpv_render_context_get_info(MpvRenderContext* ctx, MpvRenderParam param);

        /// <summary>
        /// MPV_EXPORT void mpv_render_context_set_update_callback(mpv_render_context *ctx, mpv_render_update_fn callback, void *callback_ctx);
        /// </summary>
        internal delegate void mpv_render_context_set_update_callback(MpvRenderContext* ctx, mpv_render_update_fn callback, nint callback_ctx);

        /// <summary>
        /// MPV_EXPORT uint64_t mpv_render_context_update(mpv_render_context *ctx);
        /// </summary>
        internal delegate ulong mpv_render_context_update(MpvRenderContext* ctx);

        /// <summary>
        /// MPV_EXPORT int mpv_render_context_render(mpv_render_context *ctx, mpv_render_param *params);
        /// </summary>
        internal delegate int mpv_render_context_render(MpvRenderContext* ctx, nint parameters);

        /// <summary>
        /// MPV_EXPORT void mpv_render_context_report_swap(mpv_render_context *ctx);
        /// </summary>
        internal delegate void mpv_render_context_report_swap(MpvRenderContext* ctx);

        /// <summary>
        /// MPV_EXPORT void mpv_render_context_free(mpv_render_context *ctx);
        /// </summary>
        internal delegate void mpv_render_context_free(MpvRenderContext* ctx);
    }
}
