using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

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
        MPG123_ENC_SIGNED_16   = (MPG123_ENC_16|MPG123_ENC_SIGNED|0x10),
        MPG123_ENC_UNSIGNED_16 = (MPG123_ENC_16|0x20),
        MPG123_ENC_UNSIGNED_8  = 0x01,
        MPG123_ENC_SIGNED_8    = (MPG123_ENC_SIGNED|0x02),
        MPG123_ENC_ULAW_8      = 0x04,
        MPG123_ENC_ALAW_8      = 0x08,
        MPG123_ENC_SIGNED_32   = (MPG123_ENC_32|MPG123_ENC_SIGNED|0x1000),
        MPG123_ENC_UNSIGNED_32 = (MPG123_ENC_32|0x2000),
        MPG123_ENC_SIGNED_24   = (MPG123_ENC_24|MPG123_ENC_SIGNED|0x1000),
        MPG123_ENC_UNSIGNED_24 = (MPG123_ENC_24|0x2000),
        MPG123_ENC_FLOAT_32    = 0x200,
        MPG123_ENC_FLOAT_64    = 0x400,
        MPG123_ENC_ANY = ( MPG123_ENC_SIGNED_16  | MPG123_ENC_UNSIGNED_16
                         | MPG123_ENC_UNSIGNED_8 | MPG123_ENC_SIGNED_8
                         | MPG123_ENC_ULAW_8     | MPG123_ENC_ALAW_8
                         | MPG123_ENC_SIGNED_32  | MPG123_ENC_UNSIGNED_32
                         | MPG123_ENC_SIGNED_24  | MPG123_ENC_UNSIGNED_24
                         | MPG123_ENC_FLOAT_32   | MPG123_ENC_FLOAT_64    )
    }

    public unsafe struct mpg123_fmt
    {
        long rate;
        int channels;
        int encoding;
    }
}
