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

using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Synthesis;
using BassBoom.Native.Runtime;
using System;
using System.Diagnostics;
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
        /// Absolute path to the syn123 library
        /// </summary>
        public static string LibraryPathSyn =>
            Mpg123Instance.syn123LibPath;

        /// <summary>
        /// MPG library version
        /// </summary>
        public static Version MpgLibVersion
        {
            get
            {
                uint major = 0, minor = 0, patch = 0;
                var versionHandle = NativeInit.mpg123_distversion(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);
                Debug.WriteLine($"mpg123 version: {version}");
                return new((int)major, (int)minor, (int)patch, 0);
            }
        }

        /// <summary>
        /// Output library version
        /// </summary>
        public static Version OutLibVersion
        {
            get
            {
                uint major = 0, minor = 0, patch = 0;
                var versionHandle = NativeOutputLib.out123_distversion(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);
                Debug.WriteLine($"out123 version: {version}");
                return new((int)major, (int)minor, (int)patch, 0);
            }
        }

        /// <summary>
        /// Synthesis library version
        /// </summary>
        public static Version SynLibVersion
        {
            get
            {
                uint major = 0, minor = 0, patch = 0;
                var versionHandle = NativeSynthesis.syn123_distversion(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);
                Debug.WriteLine($"syn123 version: {version}");
                return new((int)major, (int)minor, (int)patch, 0);
            }
        }

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
        /// Library name to search for upon invoking P/Invoke
        /// </summary>
        public const string LibraryNameSyn = "syn123";

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
