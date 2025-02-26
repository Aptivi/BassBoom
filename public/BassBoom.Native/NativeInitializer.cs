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

namespace BassBoom.Native
{
    /// <summary>
    /// libmpv instance class to enable Basolia to perform sound operations
    /// </summary>
    internal static unsafe class NativeInitializer
    {
        internal static string libmpvLibPath = GetLibPath("libmpv-2");
        internal static LibraryManager? libManagerMpv;
        internal const string LibcName = "libc";

        /// <summary>
        /// Absolute path to the libmpv library
        /// </summary>
        internal static string LibraryPath =>
            libmpvLibPath;

        /// <summary>
        /// Native library version
        /// </summary>
        internal static Version NativeLibVersion
        {
            get
            {
                var @delegate = GetDelegate<NativeInit.mpv_client_api_version>(libManagerMpv, nameof(NativeInit.mpv_client_api_version));
                var versionCode = @delegate.Invoke();
                Debug.WriteLine($"libmpv version code: {versionCode}");
                ulong major = versionCode >> 16;
                ulong minor = versionCode & 7;
                return new((int)major, (int)minor, 0, 0);
            }
        }

        /// <summary>
        /// Initializes the libmpv library
        /// </summary>
        internal static void InitializeLibrary() =>
            InitializeLibrary(libmpvLibPath);

        /// <summary>
        /// Initializes the libmpv library
        /// </summary>
        /// <param name="libPath">Absolute path to the libmpv library</param>
        internal static void InitializeLibrary(string libPath)
        {
            // Check to see if we have this path
            if (!File.Exists(libPath))
                throw new BasoliaNativeLibraryException($"libmpv library path {libPath} doesn't exist.");

            // Set the library path
            string oldLibPath = libmpvLibPath;
            libmpvLibPath = libPath;

            try
            {
                // Start the libraries up
                var architecture = PlatformHelper.GetArchitecture();
                if (architecture == Architecture.X86 || architecture == Architecture.Arm)
                    throw new BasoliaNativeLibraryException("32-bit platforms are no longer supported.");
                libManagerMpv = new LibraryManager(new LibraryFile(libmpvLibPath));
                libManagerMpv.LoadNativeLibrary();
            }
            catch (Exception ex)
            {
                libmpvLibPath = oldLibPath;
                throw new BasoliaNativeLibraryException($"Failed to load libraries. {libmpvLibPath}. {ex.Message}");
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
                runtimesPath += $"runtimes/osx-{lowerArch}/native/{libName}.dylib";
            else if (PlatformHelper.IsOnUnix())
                runtimesPath += $"runtimes/linux-{lowerArch}/native/{libName}.so";
            else
                runtimesPath += $"runtimes/freebsd-{lowerArch}/native/{libName}.so";
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
