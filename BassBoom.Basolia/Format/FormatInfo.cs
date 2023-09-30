
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

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Format information
    /// </summary>
    public class FormatInfo
    {
        private long rate;
        private int channels;
        private int encoding;

        public long Rate =>
            rate;

        public long Channels =>
            channels;

        public long Encoding =>
            encoding;

        internal FormatInfo(long rate, int channels, int encoding)
        {
            this.rate = rate;
            this.channels = channels;
            this.encoding = encoding;
        }
    }
}
