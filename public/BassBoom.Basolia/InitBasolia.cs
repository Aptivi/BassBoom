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
using BassBoom.Basolia.Languages;
using BassBoom.Native;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
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
        /// MPG library version
        /// </summary>
        public static Version MpgLibVersion
        {
            get
            {
                if (!BasoliaInitialized)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_VERSIONNEEDSLIB"), mpg123_errors.MPG123_NOT_INITIALIZED);
                return MpgNative.MpgLibVersion;
            }
        }

        /// <summary>
        /// Output library version
        /// </summary>
        public static Version OutLibVersion
        {
            get
            {
                if (!BasoliaInitialized)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_VERSIONNEEDSLIB"), out123_error.OUT123_ERR);
                return MpgNative.OutLibVersion;
            }
        }

        /// <summary>
        /// BassBoom's Basolia version
        /// </summary>
        public static Version BasoliaVersion =>
            Assembly.GetExecutingAssembly().GetName().Version;
        
        /// <summary>
        /// Initializes the MPG123 library for Basolia to function
        /// </summary>
        /// <param name="root">Sets the root path to the library files</param>
        public static void Init(string root = "")
        {
            if (string.IsNullOrEmpty(root))
                MpgNative.InitializeLibrary();
            else
                MpgNative.InitializeLibrary(root);
            _basoliaInited = true;
        }

        /// <summary>
        /// Checks to see if the Basolia library is loaded or not
        /// </summary>
        /// <exception cref="InvalidOperationException">Basolia didn't initialize the MPG123 library yet.</exception>
        public static void CheckInited()
        {
            if (!BasoliaInitialized)
                throw new InvalidOperationException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_EXCEPTION_NEEDSINIT"));
        }
    }
}
