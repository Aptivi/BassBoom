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

namespace BassBoom.Native.Interop.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvRenderContext
    { }

    /// <summary>
    /// Video rendering group from libmpv
    /// </summary>
    internal static unsafe class NativeRender
    {
        /// <summary>
        /// MPV_EXPORT int mpv_hook_add(mpv_handle *ctx, uint64_t reply_userdata, const char *name, int priority);
        /// </summary>
        internal delegate int mpv_hook_add(MpvHandle* ctx, ulong reply_userdata, string name, int priority);

        /// <summary>
        /// MPV_EXPORT int mpv_hook_continue(mpv_handle *ctx, uint64_t id);
        /// </summary>
        internal delegate int mpv_hook_continue(MpvHandle* ctx, ulong id);

        /// <summary>
        /// MPV_EXPORT int mpv_get_wakeup_pipe(mpv_handle *ctx);
        /// </summary>
        internal delegate int mpv_get_wakeup_pipe(MpvHandle* ctx);
    }
}
