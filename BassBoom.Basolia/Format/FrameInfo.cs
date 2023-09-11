
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Native.Interop.Analysis;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Frame information
    /// </summary>
    public class FrameInfo
    {
        private mpg123_version version;
        private int layer;
        private long rate;
        private mpg123_mode mode;
        private int mode_ext;
        private int framesize;
        private mpg123_flags flags;
        private int emphasis;
        private int bitrate;
        private int abr_rate;
        private mpg123_vbr vbr;

        public mpg123_version Version =>
            version;

        public int Layer =>
            layer;

        public long Rate =>
            rate;

        public mpg123_mode Mode =>
            mode;

        public int ModeExt =>
            mode_ext;

        public int FrameSize =>
            framesize;

        public mpg123_flags Flags =>
            flags;

        public int Emphasis =>
            emphasis;

        public int BitRate =>
            bitrate;

        public int AbrRate =>
            abr_rate;

        public mpg123_vbr Vbr =>
            vbr;

        internal FrameInfo(mpg123_version version, int layer, long rate, mpg123_mode mode, int mode_ext, int framesize, mpg123_flags flags, int emphasis, int bitrate, int abr_rate, mpg123_vbr vbr)
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
