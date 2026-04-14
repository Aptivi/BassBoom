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

using BassBoom.Native.Exceptions;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SpecProbe.Software.Platform;
using SpecProbe.Loader;
using BassBoom.Native.Languages;

namespace BassBoom.Native
{
    /// <summary>
    /// mpg123 instance class to enable Basolia to perform sound operations
    /// </summary>
    internal static class MpgNative
    {
        internal static string baseRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        internal static string mpg123LibPath = GetLibPath("mpg123");
        internal static string out123LibPath = GetLibPath("out123");
        internal static string pthreadLibPath = GetLibPath("winpthread-1");
        internal static string libcppLibPath = GetLibPath("c++");
        internal static string libunwindLibPath = GetLibPath("unwind");

        internal static LibraryManager? libManagerMpg;
        internal static LibraryManager? libManagerOut;

        internal const string LibraryName = "mpg123";
        internal const string LibraryNameOut = "out123";

        /// <summary>
        /// MPG library version
        /// </summary>
        internal static Version MpgLibVersion
        {
            get
            {
                uint major = 0, minor = 0, patch = 0;
                var @delegate = GetDelegate<NativeInit.mpg123_distversion>(libManagerMpg, nameof(NativeInit.mpg123_distversion));
                var versionHandle = @delegate.Invoke(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);
                Debug.WriteLine($"mpg123 version: {version}");
                return new((int)major, (int)minor, (int)patch, 0);
            }
        }

        /// <summary>
        /// Output library version
        /// </summary>
        internal static Version OutLibVersion
        {
            get
            {
                uint major = 0, minor = 0, patch = 0;
                var @delegate = GetDelegate<NativeOutputLib.out123_distversion>(libManagerOut, nameof(NativeOutputLib.out123_distversion));
                var versionHandle = @delegate.Invoke(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);
                Debug.WriteLine($"out123 version: {version}");
                return new((int)major, (int)minor, (int)patch, 0);
            }
        }

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        internal static void InitializeLibrary() =>
            InitializeLibrary(baseRoot);

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        /// <param name="root">Absolute path to the root directory containing library files</param>
        internal static void InitializeLibrary(string root)
        {
            // Get the library paths
            var architecture = PlatformHelper.GetArchitecture();
            mpg123LibPath = GetLibPath(root, "mpg123");
            out123LibPath = GetLibPath(root, "out123");
            pthreadLibPath = GetLibPath(root, "winpthread-1");
            libcppLibPath = GetLibPath(root, "c++");
            libunwindLibPath = GetLibPath(root, "unwind");

            // Check the main libraries
            if (!File.Exists(mpg123LibPath))
                ThrowLibraryNotFoundException(mpg123LibPath);
            if (!File.Exists(out123LibPath))
                ThrowLibraryNotFoundException(out123LibPath);

            // Check the dependencies
            if (PlatformHelper.IsOnWindows())
            {
                if (architecture == Architecture.X64 && !File.Exists(pthreadLibPath))
                    ThrowLibraryNotFoundException(pthreadLibPath);
                else if (architecture == Architecture.Arm64)
                {
                    if (!File.Exists(libcppLibPath))
                        ThrowLibraryNotFoundException(libcppLibPath);
                    if (!File.Exists(libunwindLibPath))
                        ThrowLibraryNotFoundException(libunwindLibPath);
                }
            }

            // Figure out what libraries we need to start up
            LibraryFile[] outLibraryDependencies = [];
            if (PlatformHelper.IsOnWindows())
            {
                // Windows libraries need below dependencies
                if (architecture == Architecture.X64)
                    outLibraryDependencies = [new LibraryFile(pthreadLibPath)];
                else if (architecture == Architecture.Arm64)
                    outLibraryDependencies = [new LibraryFile(libunwindLibPath), new LibraryFile(libcppLibPath)];
            }

            try
            {
                // We don't support x86 systems here
                if (architecture == Architecture.X86 || architecture == Architecture.Arm)
                    throw new BasoliaNativeLibraryException(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_32BITUNSUPPORTED"));

                // Start the libraries up
                LibraryFile[] outLibraries = [.. outLibraryDependencies, new LibraryFile(out123LibPath)];
                libManagerMpg = new LibraryManager(new LibraryFile(mpg123LibPath));
                libManagerOut = new LibraryManager(outLibraries);
                libManagerMpg.LoadNativeLibrary();
                libManagerOut.LoadNativeLibrary();

                // Tell the library the path for the modules
                string libPluginsPath = Path.GetDirectoryName(mpg123LibPath) + "/plugins/";
                int result = -1;
                if (PlatformHelper.IsOnWindows())
                    result = NativeInit._putenv_s("MPG123_MODDIR", libPluginsPath);
                else
                    result = NativeInit.setenv("MPG123_MODDIR", libPluginsPath, 1);
                if (result != 0)
                    throw new BasoliaNativeLibraryException(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_MODDIRSETFAILED"));
            }
            catch (Exception ex)
            {
                throw new BasoliaNativeLibraryException(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_LIBSLOADFAILED") + $" [{mpg123LibPath}]\n\n{ex.Message}");
            }
        }

        internal static string GetLibPath(string libName) =>
            GetLibPath(baseRoot, libName);

        internal static string GetLibPath(string root, string libName)
        {
            string genericRid = PlatformHelper.GetCurrentGenericRid();
            string extension = PlatformHelper.IsOnWindows() ? ".dll" : PlatformHelper.IsOnMacOS() ? ".dylib" : ".so";
            string runtimesPath = root + $"/runtimes/{genericRid}/native/lib{libName}{extension}";
            return runtimesPath;
        }

        internal static TDelegate GetDelegate<TDelegate>(LibraryManager? libraryManager, string function)
            where TDelegate : Delegate
        {
            if (libraryManager is null)
                throw new BasoliaNativeLibraryException(string.Format(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_DELEGATEGETFAILED_NOINIT"), function));
            return libraryManager.GetNativeMethodDelegate<TDelegate>(function) ??
                throw new BasoliaNativeLibraryException(string.Format(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_DELEGATEGETFAILED_NOTFOUND"), function));
        }

        private static void ThrowLibraryNotFoundException(string libPath) =>
            throw new BasoliaNativeLibraryException(string.Format(LanguageTools.GetLocalized("BASSBOOM_NATIVE_EXCEPTION_LIBPATHNOTFOUND"), libPath));
    }
}
