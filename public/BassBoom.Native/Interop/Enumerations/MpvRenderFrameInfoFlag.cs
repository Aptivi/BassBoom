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
    /// MPV render frame info flag
    /// </summary>
    public enum MpvRenderFrameInfoFlag
    {
        /// <summary>
        /// Flag that checks to see if there is a next frame or not.
        /// </summary>
        MPV_RENDER_FRAME_INFO_PRESENT = 1 << 0,
        /// <summary>
        /// Flag that checks to see if there is a redraw request for the current frame or not.
        /// </summary>
        MPV_RENDER_FRAME_INFO_REDRAW = 1 << 1,
        /// <summary>
        /// Flag that checks to see if there is a reproduction request for the previous frame or not, and that implies that <see cref="MPV_RENDER_FRAME_INFO_PRESENT"/> is set.
        /// </summary>
        MPV_RENDER_FRAME_INFO_REPEAT = 1 << 2,
        /// <summary>
        /// Flag that checks to see if the player timing code expects that the user thread blocks on vertical sync, and that implies that <see cref="MPV_RENDER_FRAME_INFO_PRESENT"/> is set.
        /// </summary>
        MPV_RENDER_FRAME_INFO_BLOCK_VSYNC = 1 << 3,
    }
}
