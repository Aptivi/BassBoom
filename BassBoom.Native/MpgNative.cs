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
using NativeLand;

namespace BassBoom.Native
{
    /// <summary>
    /// mpg123 instance class to enable Basolia to perform sound operations
    /// </summary>
    internal static unsafe class MpgNative
    {
        internal static string mpg123LibPath = GetLibPath("mpg123");
        internal static string out123LibPath = GetLibPath("out123");
        internal static string winpthreadsLibPath = GetLibPath("libwinpthread-1", true);

        internal static mpg123_handle* _mpg123Handle;
        internal static out123_handle* _out123Handle;

        internal static LibraryManager libManagerMpg;
        internal static LibraryManager libManagerOut;

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
                var @delegate = libManagerMpg.GetNativeMethodDelegate<NativeInit.mpg123_distversion>(nameof(NativeInit.mpg123_distversion));
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
                var @delegate = libManagerOut.GetNativeMethodDelegate<NativeOutputLib.out123_distversion>(nameof(NativeOutputLib.out123_distversion));
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
            InitializeLibrary(mpg123LibPath, out123LibPath, winpthreadsLibPath);

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        /// <param name="libPath">Absolute path to the mpg123 library</param>
        /// <param name="libPathOut">Absolute path to the out123 library</param>
        /// <param name="libPathWinpthreads">Absolute path to the libwinpthreads library</param>
        internal static void InitializeLibrary(string libPath, string libPathOut, string libPathWinpthreads)
        {
            // Check to see if we have this path
            if (!File.Exists(libPath))
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't exist.");
            if (!File.Exists(libPathOut))
                throw new BasoliaNativeLibraryException($"out123 library path {libPath} doesn't exist.");
            if (PlatformHelper.IsOnWindows() && !File.Exists(libPathWinpthreads))
                throw new BasoliaNativeLibraryException($"libwinpthread-1 library path {libPathWinpthreads} doesn't exist.");

            // Set the library path
            string oldLibPath = mpg123LibPath;
            string oldLibPathOut = out123LibPath;
            mpg123LibPath = libPath;
            out123LibPath = libPathOut;
            winpthreadsLibPath = libPathWinpthreads;

            // Start the libraries up
            libManagerMpg = new LibraryManager(
                new LibraryItem(Platform.Windows, Architecture.X86, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.X64, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.Arm, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.Arm64, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.MacOS, Architecture.X64, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.MacOS, Architecture.Arm64, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.X64, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.X86, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.Arm, new LibraryFile(mpg123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.Arm64, new LibraryFile(mpg123LibPath)));
            libManagerOut = new LibraryManager(
                new LibraryItem(Platform.Windows, Architecture.X86, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.X64, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.Arm, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Windows, Architecture.Arm64, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.MacOS, Architecture.X64, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.MacOS, Architecture.Arm64, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.X64, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.X86, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.Arm, new LibraryFile(out123LibPath)),
                new LibraryItem(Platform.Linux, Architecture.Arm64, new LibraryFile(out123LibPath)));
            if (PlatformHelper.IsOnWindows())
            {
                var libManagerWinpthread = new LibraryManager(
                    new LibraryItem(Platform.Windows, Architecture.X86, new LibraryFile(winpthreadsLibPath)),
                    new LibraryItem(Platform.Windows, Architecture.X64, new LibraryFile(winpthreadsLibPath)),
                    new LibraryItem(Platform.Windows, Architecture.Arm, new LibraryFile(winpthreadsLibPath)),
                    new LibraryItem(Platform.Windows, Architecture.Arm64, new LibraryFile(winpthreadsLibPath)));
                libManagerWinpthread.LoadNativeLibrary();
            }
            libManagerMpg.LoadNativeLibrary();
            libManagerOut.LoadNativeLibrary();

            // Tell the library the path for the modules
            string libPluginsPath = Path.GetDirectoryName(mpg123LibPath) + "/plugins/";
            if (PlatformHelper.IsOnWindows())
                Environment.SetEnvironmentVariable("MPG123_MODDIR", libPluginsPath);
            else
            {
                int result = NativeInit.setenv("MPG123_MODDIR", libPluginsPath, 1);
                if (result != 0)
                    throw new BasoliaNativeLibraryException("Can't set environment variable MPG123_MODDIR");
            }

            // Verify that we've actually loaded the library!
            try
            {
                var @delegate = libManagerMpg.GetNativeMethodDelegate<NativeInit.mpg123_new>(nameof(NativeInit.mpg123_new));
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
                var @delegate = libManagerOut.GetNativeMethodDelegate<NativeOutputLib.out123_new>(nameof(NativeOutputLib.out123_new));
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

        internal static string GetLibPath(string libName, bool winOnly = false) =>
            GetLibPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), libName, winOnly);

        internal static string GetLibPath(string root, string libName, bool winOnly = false)
        {
            if (winOnly && !PlatformHelper.IsOnWindows())
                return "";
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
    }
}
