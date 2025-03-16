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
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Init
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvHandle
    { }

    /// <summary>
    /// Initialization group from libmpv
    /// </summary>
    internal static unsafe class NativeInit
    {
        /// <summary>
        /// MPV_EXPORT unsigned long mpv_client_api_version(void);
        /// </summary>
        internal delegate ulong mpv_client_api_version();

        /// <summary>
        /// MPV_EXPORT mpv_handle *mpv_create(void);
        /// </summary>
        internal delegate MpvHandle* mpv_create();

        /// <summary>
        /// MPV_EXPORT int mpv_initialize(mpv_handle *ctx);
        /// </summary>
        internal delegate int mpv_initialize(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT void mpv_destroy(mpv_handle *ctx);
        /// </summary>
        internal delegate void mpv_destroy(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT void mpv_terminate_destroy(mpv_handle *ctx);
        /// </summary>
        internal delegate void mpv_terminate_destroy(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT void mpv_free(void *data);
        /// </summary>
        internal delegate void mpv_free(IntPtr data);

        /// <summary>
        /// MPV_EXPORT mpv_handle *mpv_create_client(mpv_handle *ctx, const char *name);
        /// </summary>
        internal delegate MpvHandle* mpv_create_client(MpvHandle* ctx, string name);

        /// <summary>
        /// MPV_EXPORT mpv_handle *mpv_create_weak_client(mpv_handle *ctx, const char *name);
        /// </summary>
        internal delegate MpvHandle* mpv_create_weak_client(MpvHandle* ctx, string name);

        /// <summary>
        /// MPV_EXPORT const char *mpv_client_name(mpv_handle *ctx);
        /// </summary>
        internal delegate nint mpv_client_name(MpvHandle* ctx);

        /// <summary>                                           
        /// MPV_EXPORT int64_t mpv_client_id(mpv_handle *ctx);
        /// </summary>
        internal delegate long mpv_client_id(MpvHandle* ctx);

        /// <summary>                                           
        /// MPV_EXPORT int mpv_load_config_file(mpv_handle *ctx, const char *filename);
        /// </summary>
        internal delegate int mpv_load_config_file(MpvHandle* ctx, string filename);

        /// <summary>
        /// MPV_EXPORT int64_t mpv_get_time_ns(mpv_handle *ctx);
        /// </summary>
        internal delegate long mpv_get_time_ns(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT int64_t mpv_get_time_us(mpv_handle *ctx);
        /// </summary>
        internal delegate long mpv_get_time_us(MpvHandle* ctx);

        /// <summary>
        /// MPV_EXPORT int mpv_command(mpv_handle *ctx, const char **args);
        /// </summary>
        internal delegate int mpv_command(MpvHandle* ctx, string args);

        /// <summary>
        /// MPV_EXPORT int mpv_command_node(mpv_handle *ctx, mpv_node *args, mpv_node *result);
        /// </summary>
        internal delegate int mpv_command_node(MpvHandle* ctx, MpvNode args, out nint result);

        /// <summary>
        /// MPV_EXPORT int mpv_command_async(mpv_handle *ctx, uint64_t reply_userdata, const char **args);
        /// </summary>
        internal delegate int mpv_command_async(MpvHandle* ctx, ulong reply_userdata, string args);

        /// <summary>
        /// MPV_EXPORT int mpv_command_node_async(mpv_handle *ctx, uint64_t reply_userdata, mpv_node *args);
        /// </summary>
        internal delegate int mpv_command_node_async(MpvHandle* ctx, ulong reply_userdata, MpvNode args);

        /// <summary>
        /// MPV_EXPORT int mpv_command_ret(mpv_handle *ctx, const char **args, mpv_node *result);
        /// </summary>
        internal delegate int mpv_command_ret(MpvHandle* ctx, string args, out nint result);

        /// <summary>
        /// MPV_EXPORT int mpv_command_string(mpv_handle *ctx, const char *args);
        /// </summary>
        internal delegate int mpv_command_string(MpvHandle* ctx, string args);

        /// <summary>
        /// MPV_EXPORT void mpv_abort_async_command(mpv_handle *ctx, uint64_t reply_userdata);
        /// </summary>
        internal delegate void mpv_abort_async_command(MpvHandle* ctx, ulong reply_userdata);

        /// <summary>
        /// int setenv(const char *name, const char *value, int overwrite);
        /// </summary>
        [DllImport(NativeInitializer.LibcName, CharSet = CharSet.Ansi)]
        internal static extern int setenv(string name, string value, int overwrite);
    }
}
