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

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Format information
    /// </summary>
    public class FormatInfo
    {
        private readonly long rate;
        private readonly int channels;
        private readonly int encoding;

        /// <summary>
        /// The bit rate
        /// </summary>
        public long Rate =>
            rate;

        /// <summary>
        /// The number of channels
        /// </summary>
        public long Channels =>
            channels;

        /// <summary>
        /// The encoding number
        /// </summary>
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
