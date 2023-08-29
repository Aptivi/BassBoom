
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Basolia.File;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback positioning tools
    /// </summary>
    public static class PlaybackPositioningTools
    {
        /// <summary>
        /// Gets the current duration of the file (samples)
        /// </summary>
        public static int GetCurrentDuration()
        {
            int length;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't play a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the length
                length = NativePositioning.mpg123_tell(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the current duration of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }
    }
}
