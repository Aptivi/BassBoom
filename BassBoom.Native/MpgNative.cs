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

namespace BassBoom.Native
{
    /// <summary>
    /// mpg123 instance class to enable Basolia to perform sound operations
    /// </summary>
    internal static unsafe class MpgNative
    {
        internal static string mpg123LibPath = GetLibPath("mpg123");
        internal static string out123LibPath = GetLibPath("out123");

        internal static mpg123_handle* _mpg123Handle;
        internal static out123_handle* _out123Handle;

        internal static LibraryManager? libManagerMpg;
        internal static LibraryManager? libManagerOut;

        internal const string LibcName = "libc";
        internal const string LibraryName = "mpg123";
        internal const string LibraryNameOut = "out123";

        /// <summary>
        /// Absolute path to the mpg123 library
        /// </summary>
        internal static string LibraryPath =>
            mpg123LibPath;

        /// <summary>
        /// Absolute path to the out123 library
        /// </summary>
        internal static string LibraryPathOut =>
            out123LibPath;

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
            InitializeLibrary(mpg123LibPath, out123LibPath);

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        /// <param name="libPath">Absolute path to the mpg123 library</param>
        /// <param name="libPathOut">Absolute path to the out123 library</param>
        internal static void InitializeLibrary(string libPath, string libPathOut)
        {
            // Check to see if we have this path
            if (!File.Exists(libPath))
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't exist.");
            if (!File.Exists(libPathOut))
                throw new BasoliaNativeLibraryException($"out123 library path {libPath} doesn't exist.");

            // Set the library path
            string oldLibPath = mpg123LibPath;
            string oldLibPathOut = out123LibPath;
            mpg123LibPath = libPath;
            out123LibPath = libPathOut;

            // Start the libraries up
            var architecture = PlatformHelper.GetArchitecture();
            if (architecture == Architecture.X86 || architecture == Architecture.Arm)
                throw new BasoliaNativeLibraryException("32-bit platforms are no longer supported.");
            libManagerMpg = new LibraryManager(new LibraryFile(mpg123LibPath));
            libManagerOut = new LibraryManager(new LibraryFile(out123LibPath));
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
                throw new BasoliaNativeLibraryException("Can't set environment variable MPG123_MODDIR");

            // Verify that we've actually loaded the library!
            try
            {
                var @delegate = GetDelegate<NativeInit.mpg123_new>(libManagerMpg, nameof(NativeInit.mpg123_new));
                var handle = @delegate.Invoke(null, null);
                Debug.WriteLine($"Verifying mpg123 version: {MpgLibVersion}");
                _mpg123Handle = handle;
            }
            catch (Exception ex)
            {
                mpg123LibPath = oldLibPath;
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't contain a valid mpg123 library. out123_distversion() was called. {ex.Message}");
            }

            // Do the same for the out123 library!
            try
            {
                var @delegate = GetDelegate<NativeOutputLib.out123_new>(libManagerOut, nameof(NativeOutputLib.out123_new));
                var handle = @delegate.Invoke();
                Debug.WriteLine($"Verifying out123 version: {OutLibVersion}");
                _out123Handle = handle;
            }
            catch (Exception ex)
            {
                out123LibPath = oldLibPathOut;
                throw new BasoliaNativeLibraryException($"out123 library path {libPathOut} doesn't contain a valid out123 library. out123_distversion() was called. {ex.Message}");
            }
        }

        internal static string GetLibPath(string libName) =>
            GetLibPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), libName);

        internal static string GetLibPath(string root, string libName)
        {
            string runtimesPath = root + "/";
            string lowerArch = RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (PlatformHelper.IsOnWindows())
                runtimesPath += $"runtimes/win-{lowerArch}/native/{libName}.dll";
            else if (PlatformHelper.IsOnMacOS())
                runtimesPath += $"runtimes/osx-{lowerArch}/native/lib{libName}.dylib";
            else if (PlatformHelper.IsOnUnix())
                runtimesPath += $"runtimes/linux-{lowerArch}/native/lib{libName}.so";
            else
                runtimesPath += $"runtimes/freebsd-{lowerArch}/native/lib{libName}.so";
            return runtimesPath;
        }

        internal static TDelegate GetDelegate<TDelegate>(LibraryManager? libraryManager, string function)
            where TDelegate : Delegate
        {
            if (libraryManager is null)
                throw new BasoliaNativeLibraryException($"Can't get delegate for {function} without initializing the library first");
            return libraryManager.GetNativeMethodDelegate<TDelegate>(function) ??
                throw new BasoliaNativeLibraryException($"Can't get delegate for {function}");
        }
    }
}
