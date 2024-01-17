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

using BassBoom.Native.Exceptions;
using BassBoom.Native.Interop;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Synthesis;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

#if !NETCOREAPP
using NativeLibraryManager;
#endif

namespace BassBoom.Native.Runtime
{
    /// <summary>
    /// mpg123 instance class to enable Basolia to perform sound operations
    /// </summary>
    public unsafe class Mpg123Instance
    {
        internal static string mpg123LibPath = GetAppropriateMpg123LibraryPath();
        internal static string out123LibPath = GetAppropriateOut123LibraryPath();
        internal static string syn123LibPath = GetAppropriateSyn123LibraryPath();

        internal static mpg123_handle* _mpg123Handle;
        internal static out123_handle* _out123Handle;
        internal static syn123_handle* _syn123Handle;

        /// <summary>
        /// Singleton of the mpg123 instance class
        /// </summary>
        public static Mpg123Instance Instance { get; } = new Mpg123Instance();

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        public static void InitializeLibrary() =>
            InitializeLibrary(mpg123LibPath, out123LibPath, syn123LibPath);

        /// <summary>
        /// Initializes the mpg123 library
        /// </summary>
        /// <param name="libPath">Absolute path to the mpg123 library</param>
        /// <param name="libPathOut">Absolute path to the out123 library</param>
        /// <param name="libPathSyn">Absolute path to the syn123 library</param>
        public static void InitializeLibrary(string libPath, string libPathOut, string libPathSyn)
        {
            // Check to see if we have this path
            if (!File.Exists(libPath))
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't exist.");
            if (!File.Exists(libPathOut))
                throw new BasoliaNativeLibraryException($"out123 library path {libPath} doesn't exist.");
            if (!File.Exists(libPathSyn))
                throw new BasoliaNativeLibraryException($"syn123 library path {libPath} doesn't exist.");

            // Set the library path
            string oldLibPath = mpg123LibPath;
            string oldLibPathOut = out123LibPath;
            string oldLibPathSyn = syn123LibPath;
            mpg123LibPath = libPath;
            out123LibPath = libPathOut;
            syn123LibPath = libPathSyn;
#if NETCOREAPP
            NativeLibrary.SetDllImportResolver(typeof(NativeInit).Assembly, ResolveLibrary);
#else
            var bytesMpg = File.ReadAllBytes(mpg123LibPath);
            var libManagerMpg = new LibraryManager(
                new LibraryItem(Platform.Windows, Bitness.x32,
                    new LibraryFile("mpg123-0.dll", bytesMpg)),
                new LibraryItem(Platform.Windows, Bitness.x64,
                    new LibraryFile("mpg123-0.dll", bytesMpg)),
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libmpg123.dylib", bytesMpg)),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libmpg123.so", bytesMpg)),
                new LibraryItem(Platform.Linux, Bitness.x32,
                    new LibraryFile("libmpg123.so", bytesMpg)));
            libManagerMpg.LoadNativeLibrary();
            var bytesOut = File.ReadAllBytes(out123LibPath);
            var libManagerOut = new LibraryManager(
                new LibraryItem(Platform.Windows, Bitness.x32,
                    new LibraryFile("out123-0.dll", bytesOut)),
                new LibraryItem(Platform.Windows, Bitness.x64,
                    new LibraryFile("out123-0.dll", bytesOut)),
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libout123.dylib", bytesOut)),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libout123.so", bytesOut)),
                new LibraryItem(Platform.Linux, Bitness.x32,
                    new LibraryFile("libout123.so", bytesOut)));
            libManagerOut.LoadNativeLibrary();
            var bytesSyn = File.ReadAllBytes(syn123LibPath);
            var libManagerSyn = new LibraryManager(
                new LibraryItem(Platform.Windows, Bitness.x32,
                    new LibraryFile("syn123-0.dll", bytesSyn)),
                new LibraryItem(Platform.Windows, Bitness.x64,
                    new LibraryFile("syn123-0.dll", bytesSyn)),
                new LibraryItem(Platform.MacOs, Bitness.x64,
                    new LibraryFile("libsyn123.dylib", bytesSyn)),
                new LibraryItem(Platform.Linux, Bitness.x64,
                    new LibraryFile("libsyn123.so", bytesSyn)),
                new LibraryItem(Platform.Linux, Bitness.x32,
                    new LibraryFile("libsyn123.so", bytesSyn)));
            libManagerSyn.LoadNativeLibrary();
#endif
            string libPluginsPath = Path.GetDirectoryName(mpg123LibPath) + "/plugins/";
            if (PlatformTools.IsOnWindows())
                Environment.SetEnvironmentVariable("MPG123_MODDIR", libPluginsPath);
            else
            {
                int result = LibraryTools.setenv("MPG123_MODDIR", libPluginsPath, 1);
                if (result != 0)
                    throw new BasoliaNativeLibraryException("Can't set environment variable MPG123_MODDIR");
            }

            // Verify that we've actually loaded the library!
            try
            {
                // mpg123 returns 0 on init.
                _ = LibraryTools.mpg123_init();
                var handle = NativeInit.mpg123_new(null, null);
                _mpg123Handle = handle;
            }
            catch (Exception ex)
            {
                mpg123LibPath = oldLibPath;
                throw new BasoliaNativeLibraryException($"mpg123 library path {libPath} doesn't contain a valid mpg123 library. mpg123_init() was called. {ex.Message}");
            }

            // Do the same for the out123 library!
            try
            {
                var handle = NativeOutputLib.out123_new();
                _out123Handle = handle;
            }
            catch (Exception ex)
            {
                out123LibPath = oldLibPathOut;
                throw new BasoliaNativeLibraryException($"out123 library path {libPathOut} doesn't contain a valid out123 library. out123_new() was called. {ex.Message}");
            }

            // Do the same for the syn123 library!
            try
            {
                uint major = 0, minor = 0, patch = 0;
                var versionHandle = NativeSynthesis.syn123_distversion(ref major, ref minor, ref patch);
                string version = Marshal.PtrToStringAnsi(versionHandle);

                // We can't init handle here, because we need values.
                Debug.WriteLine(version);
            }
            catch (Exception ex)
            {
                syn123LibPath = oldLibPathSyn;
                throw new BasoliaNativeLibraryException($"syn123 library path {libPathSyn} doesn't contain a valid syn123 library. syn123_new() was called. {ex.Message}");
            }
        }

#if NETCOREAPP
        private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == LibraryTools.LibraryName)
                libHandle = NativeLibrary.Load(mpg123LibPath);
            else if (libraryName == LibraryTools.LibraryNameOut)
                libHandle = NativeLibrary.Load(out123LibPath);
            else if (libraryName == LibraryTools.LibraryNameSyn)
                libHandle = NativeLibrary.Load(syn123LibPath);
            return libHandle;
        }
#endif

        internal static string GetAppropriateMpg123LibraryPath() =>
            GetAppropriateMpg123LibraryPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        internal static string GetAppropriateMpg123LibraryPath(string root)
        {
            string runtimesPath = root + "/";
            string lowerArch = RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (PlatformTools.IsOnWindows())
                runtimesPath += $"runtimes/win-{lowerArch}/native/mpg123-0.dll";
            else if (PlatformTools.IsOnMacOS())
                runtimesPath += $"runtimes/osx-{lowerArch}/native/libmpg123.dylib";
            else if (PlatformTools.IsOnUnix())
                runtimesPath += $"runtimes/linux-{lowerArch}/native/libmpg123.so";
            else
                runtimesPath += $"runtimes/freebsd-{lowerArch}/native/libmpg123.so";
            return runtimesPath;
        }

        internal static string GetAppropriateOut123LibraryPath() =>
            GetAppropriateOut123LibraryPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        internal static string GetAppropriateOut123LibraryPath(string root)
        {
            string runtimesPath = root + "/";
            string lowerArch = RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (PlatformTools.IsOnWindows())
                runtimesPath += $"runtimes/win-{lowerArch}/native/out123-0.dll";
            else if (PlatformTools.IsOnMacOS())
                runtimesPath += $"runtimes/osx-{lowerArch}/native/libout123.dylib";
            else if (PlatformTools.IsOnUnix())
                runtimesPath += $"runtimes/linux-{lowerArch}/native/libout123.so";
            else
                runtimesPath += $"runtimes/freebsd-{lowerArch}/native/libout123.so";
            return runtimesPath;
        }

        internal static string GetAppropriateSyn123LibraryPath() =>
            GetAppropriateSyn123LibraryPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

        internal static string GetAppropriateSyn123LibraryPath(string root)
        {
            string runtimesPath = root + "/";
            string lowerArch = RuntimeInformation.OSArchitecture.ToString().ToLower();
            if (PlatformTools.IsOnWindows())
                runtimesPath += $"runtimes/win-{lowerArch}/native/syn123-0.dll";
            else if (PlatformTools.IsOnMacOS())
                runtimesPath += $"runtimes/osx-{lowerArch}/native/libsyn123.dylib";
            else if (PlatformTools.IsOnUnix())
                runtimesPath += $"runtimes/linux-{lowerArch}/native/libsyn123.so";
            else
                runtimesPath += $"runtimes/freebsd-{lowerArch}/native/libsyn123.so";
            return runtimesPath;
        }

        internal protected Mpg123Instance()
        { }
    }
}
