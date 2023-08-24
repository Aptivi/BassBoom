using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Analysis
{
    public unsafe struct mpg123_pars
    { }

    /// <summary>
    /// Parameters group from mpg123
    /// </summary>
    public static unsafe class NativeParameters
    {
        /// <summary>
        /// MPG123_EXPORT mpg123_handle *mpg123_parnew( mpg123_pars *mp
        /// ,   const char* decoder, int *error );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_handle* mpg123_parnew(mpg123_pars* mp, string decoder, int* error);

        /// <summary>
        /// MPG123_EXPORT mpg123_pars *mpg123_new_pars(int *error);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern mpg123_pars *mpg123_new_pars(int *error);

        /// <summary>
        /// MPG123_EXPORT void mpg123_delete_pars(mpg123_pars* mp);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern void mpg123_delete_pars(mpg123_pars* mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_none(mpg123_pars *mp);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_none(mpg123_pars *mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_all(mpg123_pars *mp);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_all(mpg123_pars *mp);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt(mpg123_pars *mp
        /// ,   long rate, int channels, int encodings);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt(mpg123_pars* mp, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt2(mpg123_pars *mp
        /// ,   long rate, int channels, int encodings);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt2(mpg123_pars* mp, long rate, int channels, int encodings);

        /// <summary>
        /// MPG123_EXPORT int mpg123_fmt_support(mpg123_pars *mp, long rate, int encoding);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_fmt_support(mpg123_pars *mp, long rate, int encoding);

        /// <summary>
        /// MPG123_EXPORT int mpg123_par( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long value, double fvalue );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_par(mpg123_pars* mp, mpg123_parms type, long @value, double fvalue);

        /// <summary>
        /// mpg123_par( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long value, double fvalue );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_par2(mpg123_pars* mp, int type, long @value, double fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getpar( mpg123_pars *mp
        /// ,   enum mpg123_parms type, long *value, double *fvalue );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getpar(mpg123_pars* mp, mpg123_parms type, long* @value, double* fvalue);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getpar2( mpg123_pars *mp
        /// ,   int type, long *value, double *fvalue );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getpar2(mpg123_pars* mp, int type, long* @value, double* fvalue);
    }
}
