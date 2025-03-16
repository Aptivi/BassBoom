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
    [StructLayout(LayoutKind.Sequential)]
    internal struct MpvEventLogMessage
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string prefix;
        [MarshalAs(UnmanagedType.LPStr)]
        public string level;
        [MarshalAs(UnmanagedType.LPStr)]
        public string text;
        public MpvLogLevel log_level;
    }

    /// <summary>
    /// Logging group from libmpv
    /// </summary>
    internal static unsafe class NativeLogging
    {
        /// <summary>
        /// MPV_EXPORT int mpv_request_log_messages(mpv_handle *ctx, const char *min_level);
        /// </summary>
        internal delegate int mpv_request_log_messages(MpvHandle* ctx, string min_level);
    }
}
