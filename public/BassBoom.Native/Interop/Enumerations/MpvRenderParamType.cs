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
        /// Defines the render API to use (string)
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
        /// OpenGL DRM display (mpv_opengl_drm_params_v2*)
        /// </summary>
        MPV_RENDER_PARAM_DRM_DISPLAY_V2 = 16,
        /// <summary>
        /// Target surface size for rendering using the software renderer (int[2])
        /// </summary>
        MPV_RENDER_PARAM_SW_SIZE = 17,
        /**
         * MPV_RENDER_API_TYPE_SW only: rendering target surface pixel format,
         * mandatory.
         * Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render().
         * Type: char* (e.g.: char *f = "rgb0"; param.data = f;)
         *
         * Valid values are:
         *  "rgb0", "bgr0", "0bgr", "0rgb"
         *      4 bytes per pixel RGB, 1 byte (8 bit) per component, component bytes
         *      with increasing address from left to right (e.g. "rgb0" has r at
         *      address 0), the "0" component contains uninitialized garbage (often
         *      the value 0, but not necessarily; the bad naming is inherited from
         *      FFmpeg)
         *      Pixel alignment size: 4 bytes
         *  "rgb24"
         *      3 bytes per pixel RGB. This is strongly discouraged because it is
         *      very slow.
         *      Pixel alignment size: 1 bytes
         *  other
         *      The API may accept other pixel formats, using mpv internal format
         *      names, as long as it's internally marked as RGB, has exactly 1
         *      plane, and is supported as conversion output. It is not a good idea
         *      to rely on any of these. Their semantics and handling could change.
         */
        MPV_RENDER_PARAM_SW_FORMAT = 18,
        /**
         * MPV_RENDER_API_TYPE_SW only: rendering target surface bytes per line,
         * mandatory.
         * Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render().
         * Type: size_t*
         *
         * This is the number of bytes between a pixel (x, y) and (x, y + 1) on the
         * target surface. It must be a multiple of the pixel size, and have space
         * for the surface width as specified by MPV_RENDER_PARAM_SW_SIZE.
         *
         * Both stride and pointer value should be a multiple of 64 to facilitate
         * fast SIMD operation. Lower alignment might trigger slower code paths,
         * and in the worst case, will copy the entire target frame. If mpv is built
         * with zimg (and zimg is not disabled), the performance impact might be
         * less.
         * In either cases, the pointer and stride must be aligned at least to the
         * pixel alignment size. Otherwise, crashes and undefined behavior is
         * possible on platforms which do not support unaligned accesses (either
         * through normal memory access or aligned SIMD memory access instructions).
         */
        MPV_RENDER_PARAM_SW_STRIDE = 19,
        /*
         * MPV_RENDER_API_TYPE_SW only: rendering target surface pixel data pointer,
         * mandatory.
         * Valid for MPV_RENDER_API_TYPE_SW & mpv_render_context_render().
         * Type: void*
         *
         * This points to the first pixel at the left/top corner (0, 0). In
         * particular, each line y starts at (pointer + stride * y). Upon rendering,
         * all data between pointer and (pointer + stride * h) is overwritten.
         * Whether the padding between (w, y) and (0, y + 1) is overwritten is left
         * unspecified (it should not be, but unfortunately some scaler backends
         * will do it anyway). It is assumed that even the padding after the last
         * line (starting at bytepos(w, h) until (pointer + stride * h)) is
         * writable.
         *
         * See MPV_RENDER_PARAM_SW_STRIDE for alignment requirements.
         */
        MPV_RENDER_PARAM_SW_POINTER = 20,
    }
}
