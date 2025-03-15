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

using BassBoom.Native.Interop.Init;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Play
{
    internal enum mpg123_channels
    {
        MPG123_LEFT = 0x1,
        MPG123_RIGHT = 0x2,
        MPG123_LR = 0x3
    }

    /// <summary>
    /// Volume group from mpg123
    /// </summary>
    internal static unsafe class NativeVolume
    {
        /// <summary>
        /// MPG123_EXPORT int mpg123_eq( mpg123_handle *mh
        /// ,   enum mpg123_channels channel, int band, double val );
        /// </summary>
        internal delegate int mpg123_eq(mpg123_handle* mh, mpg123_channels channel, int band, double val);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq2( mpg123_handle *mh
        /// ,   int channel, int band, double val );
        /// </summary>
        internal delegate int mpg123_eq2(mpg123_handle* mh, int channel, int band, double val);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq_bands( mpg123_handle *mh
        /// ,   int channel, int a, int b, double factor );
        /// </summary>
        internal delegate int mpg123_eq_bands(mpg123_handle* mh, int channel, int a, int b, double factor);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq_change( mpg123_handle *mh
        /// ,   int channel, int a, int b, double db );
        /// </summary>
        internal delegate int mpg123_eq_change(mpg123_handle* mh, int channel, int a, int b, double db);

        /// <summary>
        /// MPG123_EXPORT double mpg123_geteq(mpg123_handle *mh
        /// , enum mpg123_channels channel, int band);
        /// </summary>
        internal delegate double mpg123_geteq(mpg123_handle* mh, mpg123_channels channel, int band);

        /// <summary>
        /// MPG123_EXPORT double mpg123_geteq2(mpg123_handle *mh, int channel, int band);
        /// </summary>
        internal delegate double mpg123_geteq2(mpg123_handle* mh, int channel, int band);

        /// <summary>
        /// MPG123_EXPORT int mpg123_reset_eq(mpg123_handle *mh);
        /// </summary>
        internal delegate int mpg123_reset_eq(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume(mpg123_handle *mh, double vol);
        /// </summary>
        internal delegate int mpg123_volume(mpg123_handle* mh, double vol);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume_change(mpg123_handle *mh, double change);
        /// </summary>
        internal delegate int mpg123_volume_change(mpg123_handle *mh, double change);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume_change_db(mpg123_handle *mh, double db);
        /// </summary>
        internal delegate int mpg123_volume_change_db(mpg123_handle *mh, double db);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getvolume(mpg123_handle *mh, double *base, double *really, double *rva_db);
        /// </summary>
        internal delegate int mpg123_getvolume(mpg123_handle* mh, ref double @base, ref double really, ref double rva_db);
    }
}
