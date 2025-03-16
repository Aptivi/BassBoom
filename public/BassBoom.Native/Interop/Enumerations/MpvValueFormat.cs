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
    /// MPV value formatting options
    /// </summary>
    public enum MpvValueFormat
    {
        /// <summary>
        /// No format, usually 0.
        /// </summary>
        MPV_FORMAT_NONE = 0,
        /// <summary>
        /// String type (const char* in C, <see cref="nint"/> converted to <see cref="string"/> in C#)
        /// </summary>
        MPV_FORMAT_STRING = 1,
        /// <summary>
        /// Human-readable string type (const char* in C, <see cref="nint"/> converted to <see cref="string"/> in C#)
        /// </summary>
        MPV_FORMAT_OSD_STRING = 2,
        /// <summary>
        /// Boolean type (0 is false, 1 is true)
        /// </summary>
        MPV_FORMAT_FLAG = 3,
        /// <summary>
        /// 64-bit integer type (int64_t in C, <see cref="long"/> in C#)
        /// </summary>
        MPV_FORMAT_INT64 = 4,
        /// <summary>
        /// Double-precision floating point number type (double in C, <see cref="double"/> in C#)
        /// </summary>
        MPV_FORMAT_DOUBLE = 5,
        /// <summary>
        /// MPV node type
        /// </summary>
        MPV_FORMAT_NODE = 6,
        /// <summary>
        /// MPV node array type (used within node type)
        /// </summary>
        MPV_FORMAT_NODE_ARRAY = 7,
        /// <summary>
        /// MPV node array type (used within node type)
        /// </summary>
        MPV_FORMAT_NODE_MAP = 8,
        /// <summary>
        /// Raw byte array (used within node type, <see cref="byte"/>[] in C#)
        /// </summary>
        MPV_FORMAT_BYTE_ARRAY = 9
    }
}
