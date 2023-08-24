using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop.Init
{
    /// <summary>
    /// Decoder group from mpg123
    /// </summary>
    public static unsafe class NativeDecoder
    {
        /// <summary>
        /// MPG123_EXPORT const char **mpg123_decoders(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string[] mpg123_decoders();

        /// <summary>
        /// MPG123_EXPORT const char **mpg123_supported_decoders(void);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string[] mpg123_supported_decoders();

        /// <summary>
        /// MPG123_EXPORT int mpg123_decoder(mpg123_handle *mh, const char* decoder_name);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_decoder(mpg123_handle* mh, string decoder_name);

        /// <summary>
        /// MPG123_EXPORT const char* mpg123_current_decoder(mpg123_handle *mh);
        /// </summary>
        [DllImport(LibraryTools.LibraryName, CharSet = CharSet.Ansi)]
        internal static extern string mpg123_current_decoder(mpg123_handle* mh);
    }
}
