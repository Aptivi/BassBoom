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

namespace BassBoom.Native.Interop.Enumerations
{
    /// <summary>
    /// MPV render parameter types
    /// </summary>
    public enum MpvRenderParamType
    {
        /// <summary>
        /// Not a valid value (always 0).
        /// </summary>
        MPV_RENDER_PARAM_INVALID = 0,
        /// <summary>
        /// Defines the render API to use (string ("opengl", "sw"))
        /// </summary>
        MPV_RENDER_PARAM_API_TYPE = 1,
        /// <summary>
        /// OpenGL initialization parameters (mpv_opengl_init_params struct passed as <see cref="nint"/> in C#)
        /// </summary>
        MPV_RENDER_PARAM_OPENGL_INIT_PARAMS = 2,
        /// <summary>
        /// OpenGL FBO render target (mpv_opengl_fbo struct passed as <see cref="nint"/> in C#)
        /// </summary>
        MPV_RENDER_PARAM_OPENGL_FBO = 3,
        /// <summary>
        /// Flip the Y coordinates (int, 0 means don't flip Y, non-zero number means flip Y)
        /// </summary>
        MPV_RENDER_PARAM_FLIP_Y = 4,
        /// <summary>
        /// Control surface depth in bits per channel (int, 0 means 8 bits, non-zero number means selected choice)
        /// </summary>
        MPV_RENDER_PARAM_DEPTH = 5,
        /// <summary>
        /// ICC profile blob (mpv_byte_array struct passed as <see cref="nint"/> in C#)
        /// </summary>
        MPV_RENDER_PARAM_ICC_PROFILE = 6,
        /// <summary>
        /// Ambient light in lux (int)
        /// </summary>
        MPV_RENDER_PARAM_AMBIENT_LIGHT = 7,
        /// <summary>
        /// X11 Display (Display*)
        /// </summary>
        MPV_RENDER_PARAM_X11_DISPLAY = 8,
        /// <summary>
        /// Wayland Display (wl_display*)
        /// </summary>
        MPV_RENDER_PARAM_WL_DISPLAY = 9,
        /// <summary>
        /// Advanced control flag (0 is false, 1 is true)
        /// </summary>
        MPV_RENDER_PARAM_ADVANCED_CONTROL = 10,
        /// <summary>
        /// Next frame information to render (mpv_render_frame_info*)
        /// </summary>
        MPV_RENDER_PARAM_NEXT_FRAME_INFO = 11,
        /// <summary>
        /// Video timing flag (0 is false, 1 is true)
        /// </summary>
        MPV_RENDER_PARAM_BLOCK_FOR_TARGET_TIME = 12,
        /// <summary>
        /// Skip rendering flag (0 is false, 1 is true)
        /// </summary>
        MPV_RENDER_PARAM_SKIP_RENDERING = 13,
        /// <summary>
        /// OpenGL DRM display (mpv_opengl_drm_params*, deprecated)
        /// </summary>
        MPV_RENDER_PARAM_DRM_DISPLAY = 14,
        /// <summary>
        /// OpenGL DRM draw surface size (mpv_opengl_drm_draw_surface_size*)
        /// </summary>
        MPV_RENDER_PARAM_DRM_DRAW_SURFACE_SIZE = 15,
        /// <summary>
        /// OpenGL DRM draw surface size (mpv_opengl_drm_draw_surface_size*)
        /// </summary>
        MPV_RENDER_PARAM_DRM_OSD_SIZE = 15,
        /// <summary>
        /// OpenGL DRM display (mpv_opengl_drm_params_v2*)
        /// </summary>
        MPV_RENDER_PARAM_DRM_DISPLAY_V2 = 16,
        /// <summary>
        /// Target surface size for rendering using the software renderer (int[2])
        /// </summary>
        MPV_RENDER_PARAM_SW_SIZE = 17,
        /// <summary>
        /// Target surface pixel format for rendering using the software renderer (string ("rgb0", "bgr0", "0bgr", "0rgb", "rgb24", or any other format))
        /// </summary>
        MPV_RENDER_PARAM_SW_FORMAT = 18,
        /// <summary>
        /// Target surface bytes per line for rendering using the software renderer (int)
        /// </summary>
        MPV_RENDER_PARAM_SW_STRIDE = 19,
        /// <summary>
        /// Target surface pixel data pointer for rendering using the software renderer (void* passed as <see cref="nint"/> in C#)
        /// </summary>
        MPV_RENDER_PARAM_SW_POINTER = 20,
    }
}
