using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Play
{
    public enum mpg123_channels
    {
        MPG123_LEFT = 0x1,
        MPG123_RIGHT = 0x2,
        MPG123_LR = 0x3
    }

    /// <summary>
    /// Volume group from mpg123
    /// </summary>
    public static unsafe class NativeVolume
    {
        /// <summary>
        /// MPG123_EXPORT int mpg123_eq( mpg123_handle *mh
        /// ,   enum mpg123_channels channel, int band, double val );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_eq(mpg123_handle* mh, mpg123_channels channel, int band, double val);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq2( mpg123_handle *mh
        /// ,   int channel, int band, double val );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_eq2(mpg123_handle* mh, int channel, int band, double val);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq_bands( mpg123_handle *mh
        /// ,   int channel, int a, int b, double factor );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_eq_bands(mpg123_handle* mh, int channel, int a, int b, double factor);

        /// <summary>
        /// MPG123_EXPORT int mpg123_eq_change( mpg123_handle *mh
        /// ,   int channel, int a, int b, double db );
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_eq_change(mpg123_handle* mh, int channel, int a, int b, double db);

        /// <summary>
        /// MPG123_EXPORT double mpg123_geteq(mpg123_handle *mh
        /// , enum mpg123_channels channel, int band);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern double mpg123_geteq(mpg123_handle* mh, mpg123_channels channel, int band);

        /// <summary>
        /// MPG123_EXPORT double mpg123_geteq2(mpg123_handle *mh, int channel, int band);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern double mpg123_geteq2(mpg123_handle* mh, int channel, int band);

        /// <summary>
        /// MPG123_EXPORT int mpg123_reset_eq(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_reset_eq(mpg123_handle* mh);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume(mpg123_handle *mh, double vol);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_volume(mpg123_handle* mh, double vol);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume_change(mpg123_handle *mh, double change);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_volume_change(mpg123_handle *mh, double change);

        /// <summary>
        /// MPG123_EXPORT int mpg123_volume_change_db(mpg123_handle *mh, double db);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_volume_change_db(mpg123_handle *mh, double db);

        /// <summary>
        /// MPG123_EXPORT int mpg123_getvolume(mpg123_handle *mh, double *base, double *really, double *rva_db);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_getvolume(mpg123_handle* mh, double* @base, double* really, double* rva_db);
    }
}
