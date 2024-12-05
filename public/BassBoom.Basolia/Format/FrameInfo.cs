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

using BassBoom.Basolia.Enumerations;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Frame information
    /// </summary>
    public class FrameInfo
    {
        private readonly FrameVersion version;
        private readonly int layer;
        private readonly long rate;
        private readonly FrameMode mode;
        private readonly int mode_ext;
        private readonly int framesize;
        private readonly FrameFlags flags;
        private readonly int emphasis;
        private readonly int bitrate;
        private readonly int abr_rate;
        private readonly FrameVbr vbr;

        /// <summary>
        /// MPEG version
        /// </summary>
        public FrameVersion Version =>
            version;

        /// <summary>
        /// MPEG layer
        /// </summary>
        public int Layer =>
            layer;

        /// <summary>
        /// Bit rate
        /// </summary>
        public long Rate =>
            rate;

        /// <summary>
        /// Stereo mode
        /// </summary>
        public FrameMode Mode =>
            mode;

        /// <summary>
        /// Stereo mode (extended)
        /// </summary>
        public int ModeExt =>
            mode_ext;

        /// <summary>
        /// Frame size
        /// </summary>
        public int FrameSize =>
            framesize;

        /// <summary>
        /// Music file flags
        /// </summary>
        public FrameFlags Flags =>
            flags;

        /// <summary>
        /// Emphasis
        /// </summary>
        public int Emphasis =>
            emphasis;

        /// <summary>
        /// Bit rate
        /// </summary>
        public int BitRate =>
            bitrate;

        /// <summary>
        /// ABR rate
        /// </summary>
        public int AbrRate =>
            abr_rate;

        /// <summary>
        /// Variable bit rate mode
        /// </summary>
        public FrameVbr Vbr =>
            vbr;

        internal FrameInfo(FrameVersion version, int layer, long rate, FrameMode mode, int mode_ext, int framesize, FrameFlags flags, int emphasis, int bitrate, int abr_rate, FrameVbr vbr)
        {
            this.version = version;
            this.layer = layer;
            this.rate = rate;
            this.mode = mode;
            this.mode_ext = mode_ext;
            this.framesize = framesize;
            this.flags = flags;
            this.emphasis = emphasis;
            this.bitrate = bitrate;
            this.abr_rate = abr_rate;
            this.vbr = vbr;
        }
    }
}
