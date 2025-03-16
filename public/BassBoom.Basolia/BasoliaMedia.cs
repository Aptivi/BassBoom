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

using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback;
using BassBoom.Native;
using BassBoom.Native.Exceptions;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using System;
using System.Diagnostics;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Basolia instance for media manipulation
    /// </summary>
    public unsafe class BasoliaMedia
    {
        internal bool bufferPlaying = false;
        internal bool holding = false;
        internal string radioIcy = "";
        internal PlaybackState state = PlaybackState.Stopped;
        internal string? activeDriver;
        internal string? activeDevice;
        internal bool isOpened = false;
        internal bool isRadioStation = false;
        internal bool isOutputOpen = false;
        internal FileType? currentFile;

        internal MpvHandle* _libmpvHandle;

        /// <summary>
        /// Makes a new Basolia instance and initializes the library, if necessary.
        /// </summary>
        /// <param name="root">Root directory that contains native library files</param>
        /// <exception cref="BasoliaNativeLibraryException"></exception>
        public BasoliaMedia(string root = "")
        {
            if (!InitBasolia.BasoliaInitialized)
                InitBasolia.Init(root);

            // Verify that we've actually loaded the library!
            try
            {
                var @delegate = NativeInitializer.GetDelegate<NativeInit.mpv_create>(NativeInitializer.libManagerMpv, nameof(NativeInit.mpv_create));
                var handle = @delegate.Invoke();
                Debug.WriteLine($"Verifying libmpv version: {NativeInitializer.NativeLibVersion}");
                _libmpvHandle = handle;
            }
            catch (Exception ex)
            {
                throw new BasoliaNativeLibraryException($"libmpv library path {NativeInitializer.libmpvLibPath} doesn't contain a valid libmpv library. mpv_create() was called. {ex.Message}");
            }
        }
    }
}
