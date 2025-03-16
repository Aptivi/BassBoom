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

using BassBoom.Basolia.Exceptions;
using BassBoom.Native;
using BassBoom.Native.Interop.Enumerations;
using System;
using System.Reflection;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Initialization code
    /// </summary>
    public static class InitBasolia
    {
        private static bool _basoliaInited = false;

        /// <summary>
        /// Whether the Basolia library has been initialized or not
        /// </summary>
        public static bool BasoliaInitialized =>
            _basoliaInited;

        /// <summary>
        /// Native library version
        /// </summary>
        public static Version NativeLibVersion
        {
            get
            {
                if (!BasoliaInitialized)
                    throw new BasoliaException("Can't get version until the library is initialized", MpvError.MPV_ERROR_UNINITIALIZED);
                return NativeInitializer.NativeLibVersion;
            }
        }

        /// <summary>
        /// BassBoom's Basolia version
        /// </summary>
        public static Version BasoliaVersion =>
            Assembly.GetExecutingAssembly().GetName().Version;
        
        /// <summary>
        /// Initializes the libmpv library for Basolia to function
        /// </summary>
        /// <param name="root">Sets the root path to the library files</param>
        public static void Init(string root = "")
        {
            if (string.IsNullOrEmpty(root))
                NativeInitializer.InitializeLibrary();
            else
            {
                string mpv = NativeInitializer.GetLibPath(root, "libmpv-2");
                NativeInitializer.InitializeLibrary(mpv);
            }
            _basoliaInited = true;
        }

        /// <summary>
        /// Checks to see if the Basolia library is loaded or not
        /// </summary>
        /// <exception cref="InvalidOperationException">Basolia didn't initialize the libmpv library yet.</exception>
        public static void CheckInited()
        {
            if (!BasoliaInitialized)
                throw new InvalidOperationException("Basolia didn't initialize the libmpv library yet!");
        }
    }
}
