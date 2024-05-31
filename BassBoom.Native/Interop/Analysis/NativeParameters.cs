//
// BassBoom  Copyright (C) 2023  Aptivi
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

using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Analysis
{
    internal unsafe struct mpg123_pars
    { }

    /// <summary>
    /// Parameters group from mpg123
    /// </summary>
    internal static unsafe class NativeParameters
    {
        /// <summary>
        /// MPG123_EXPORT mpg123_handle *mpg123_parnew( mpg123_pars *mp
        /// ,   const char* decoder, int *error );
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_handle* mpg123_parnew(mpg123_pars* mp, string decoder, int* error);

        /// <summary>
        /// MPG123_EXPORT mpg123_pars *mpg123_new_pars(int *error);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_pars *mpg123_new_pars(int *error);

        /// <summary>
        /// MPG123_EXPORT void mpg123_delete_pars(mpg123_pars* mp);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_delete_pars(mpg123_pars* mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_none(mpg123_pars *mp);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_none(mpg123_pars *mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_all(mpg123_pars *mp);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_all(mpg123_pars *mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt(mpg123_pars *mp
        /// ,   long rate, int channels, int encodings);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt(mpg123_pars* mp, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt2(mpg123_pars *mp
        /// ,   long rate, int channels, int encodings);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt2(mpg123_pars* mp, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_support(mpg123_pars *mp, long rate, int encoding);
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_support(mpg123_pars *mp, long rate, int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_par( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long value, double fvalue );
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_par(mpg123_pars* mp, mpg123_parms type, long @value, double fvalue);

        /// <summary>
        /// mpg123_par( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long value, double fvalue );
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_par2(mpg123_pars* mp, int type, long @value, double fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getpar( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long *value, double *fvalue );
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getpar(mpg123_pars* mp, mpg123_parms type, long* @value, double* fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getpar2( mpg123_pars *mp
        /// ,   int type, long *value, double *fvalue );
        /// </summary>
        [DllImport(MpgNative.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getpar2(mpg123_pars* mp, int type, long* @value, double* fvalue);
    }
}
