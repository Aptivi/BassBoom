//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
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

namespace BassBoom.Native.Interop.Analysis
{
    public enum mpg123_enc_enum
    {
        MPG123_ENC_8      = 0x00f,
        MPG123_ENC_16     = 0x040,
        MPG123_ENC_24     = 0x4000,
        MPG123_ENC_32     = 0x100,
        MPG123_ENC_SIGNED = 0x080,
        MPG123_ENC_FLOAT  = 0xe00,
        MPG123_ENC_SIGNED_16   = MPG123_ENC_16|MPG123_ENC_SIGNED|0x10,
        MPG123_ENC_UNSIGNED_16 = MPG123_ENC_16|0x20,
        MPG123_ENC_UNSIGNED_8  = 0x01,
        MPG123_ENC_SIGNED_8    = MPG123_ENC_SIGNED|0x02,
        MPG123_ENC_ULAW_8      = 0x04,
        MPG123_ENC_ALAW_8      = 0x08,
        MPG123_ENC_SIGNED_32   = MPG123_ENC_32|MPG123_ENC_SIGNED|0x1000,
        MPG123_ENC_UNSIGNED_32 = MPG123_ENC_32|0x2000,
        MPG123_ENC_SIGNED_24   = MPG123_ENC_24|MPG123_ENC_SIGNED|0x1000,
        MPG123_ENC_UNSIGNED_24 = MPG123_ENC_24|0x2000,
        MPG123_ENC_FLOAT_32    = 0x200,
        MPG123_ENC_FLOAT_64    = 0x400,
        MPG123_ENC_ANY =   MPG123_ENC_SIGNED_16  | MPG123_ENC_UNSIGNED_16
                         | MPG123_ENC_UNSIGNED_8 | MPG123_ENC_SIGNED_8
                         | MPG123_ENC_ULAW_8     | MPG123_ENC_ALAW_8
                         | MPG123_ENC_SIGNED_32  | MPG123_ENC_UNSIGNED_32
                         | MPG123_ENC_SIGNED_24  | MPG123_ENC_UNSIGNED_24
                         | MPG123_ENC_FLOAT_32   | MPG123_ENC_FLOAT_64
    }

    public unsafe struct mpg123_fmt
    {
        internal long rate;
        internal int channels;
        internal int encoding;
    }

    public unsafe struct mpg123_fmt_win
    {
        internal int rate;
        internal int channels;
        internal int encoding;
    }
}
