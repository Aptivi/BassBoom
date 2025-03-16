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
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Analysis
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MpvNodeUnion
    {
        [FieldOffset(0)]
        public System.IntPtr @string;
        [FieldOffset(0)]
        public int flag;
        [FieldOffset(0)]
        public int int64;
        [FieldOffset(0)]
        public double double_;
        [FieldOffset(0)]
        public System.IntPtr list;
        [FieldOffset(0)]
        public System.IntPtr ba;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvNode
    {
        public MpvNodeUnion u;
        public MpvValueFormat format;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvNodeList
    {
        public int num;
        public System.IntPtr values;
        public System.IntPtr keys;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvNodeByteArray
    {
        public System.IntPtr data;
        public int size;
    }

    /// <summary>
    /// Node format group from libmpv
    /// </summary>
    internal static unsafe class NativeNodeFormats
    {
        /// <summary>
        /// MPV_EXPORT void mpv_free_node_contents(mpv_node *node);
        /// </summary>
        internal delegate void mpv_free_node_contents(MpvNode* node);
    }
}
