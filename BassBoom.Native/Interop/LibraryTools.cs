using BassBoom.Native.Runtime;
using System.Runtime.InteropServices;

namespace BassBoom.Native.Interop
{
    /// <summary>
    /// General native library properties and functions
    /// </summary>
    public static unsafe class LibraryTools
    {
        /// <summary>
        /// Absolute path to the mpg123 library
        /// </summary>
        public static string LibraryPath =>
            Mpg123Instance.mpg123LibPath;

        /// <summary>
        /// Absolute path to the out123 library
        /// </summary>
        public static string LibraryPathOut =>
            Mpg123Instance.out123LibPath;

        /// <summary>
        /// C library name (POSIX)
        /// </summary>
        public const string LibcName = "libc";

        /// <summary>
        /// Library name to search for upon invoking P/Invoke
        /// </summary>
        public const string LibraryName = "mpg123";

        /// <summary>
        /// Library name to search for upon invoking P/Invoke
        /// </summary>
        public const string LibraryNameOut = "out123";

        /// <summary>
        /// MPG123_EXPORT int mpg123_init (void)
        /// </summary>
        [DllImport(LibraryName, CharSet = CharSet.Ansi)]
        internal static extern int mpg123_init();

        /// <summary>
        /// int setenv(const char *name, const char *value, int overwrite);
        /// </summary>
        [DllImport(LibcName, CharSet = CharSet.Ansi)]
        internal static extern int setenv(string name, string value, int overwrite);

        /// <summary>
        /// int unsetenv(const char *name);
        /// </summary>
        [DllImport(LibcName, CharSet = CharSet.Ansi)]
        internal static extern int unsetenv(string name);
    }
}
