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

using System;
using BassBoom.Native.Interop.Analysis;

namespace BassBoom.Basolia.Media.Format
{
    /// <summary>
    /// Audio format tools
    /// </summary>
    public static class FormatTools
    {
        /// <summary>
        /// Gets the duration from the number of samples
        /// </summary>
        /// <param name="samples">Number of samples</param>
        /// <param name="rate">Bit rate</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public static TimeSpan GetDurationSpanFromSamples(int samples, long rate)
        {
            // Get the required values
            long seconds = samples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Gets a PCM sample size for a given encoding (library doesn't need to be initialized)
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <returns>Sample size in bytes</returns>
        public static int GetSampleSize(int encoding)
        {
            int sampleSize = 0;
            var enumEncoding = (mpg123_enc_enum)encoding;
            if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_8))
                sampleSize = 1;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_16))
                sampleSize = 2;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_24))
                sampleSize = 3;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_32) ||
                enumEncoding == mpg123_enc_enum.MPG123_ENC_FLOAT_32)
                sampleSize = 4;
            else if (enumEncoding == mpg123_enc_enum.MPG123_ENC_FLOAT_64)
                sampleSize = 8;
            return sampleSize;
        }

        /// <summary>
        /// Gets a zero sample representation
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <param name="sampleSize">Sample size in bytes. See <see cref="GetSampleSize(int)"/></param>
        /// <param name="lsbOffset">LSB offset in bytes</param>
        /// <returns>Zero sample size in bytes</returns>
        public static int GetZeroSample(int encoding, int sampleSize, int lsbOffset)
        {
            var enumEncoding = (mpg123_enc_enum)encoding;
            if (enumEncoding == mpg123_enc_enum.MPG123_ENC_ULAW_8)
                return lsbOffset == 0 ? 0xff : 0x00;
            else if (enumEncoding == mpg123_enc_enum.MPG123_ENC_ALAW_8)
                return lsbOffset == 0 ? 0xd5 : 0x00;
            else if ((enumEncoding & (mpg123_enc_enum.MPG123_ENC_SIGNED | mpg123_enc_enum.MPG123_ENC_FLOAT)) > 0 ||
                sampleSize != (lsbOffset + 1))
                return 0x00;
            return 0x80;
        }
    }
}
