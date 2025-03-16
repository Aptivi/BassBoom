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
    internal delegate nint mpv_opengl_init_params_get_proc_address(nint ctx, [In][MarshalAs(UnmanagedType.LPStr)] string name);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvOpenGLInitParams
    {
        public mpv_opengl_init_params_get_proc_address get_proc_address;
        public nint get_proc_address_ctx;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvOpenGLFbo
    {
        public int fbo;
        public int w;
        public int h;
        public int internal_format;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvOpenGLDrmParams
    {
        public int fd;
        public int crtc_id;
        public int connector_id;
        public nint atomic_request_ptr;
        public int render_fd;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvOpenGLDrmDrawSurfaceSize
    {
        public int width;
        public int height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvOpenGLDrmParamsV2
    {
        public int fd;
        public int crtc_id;
        public int connector_id;
        public nint atomic_request_ptr;
        public int render_fd;
    }
}
