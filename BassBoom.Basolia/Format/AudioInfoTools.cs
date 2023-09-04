
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

using BassBoom.Native.Runtime;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Init;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback;
using System;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Audio information tools
    /// </summary>
    public static class AudioInfoTools
    {
        /// <summary>
        /// Gets the duration of the file
        /// </summary>
        public static int GetDuration(bool scan)
        {
            int length;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't play a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing && !InitBasolia._fugitive)
                throw new BasoliaException("Trying to get the duration during playback causes playback corruption! Don't call this function during playback. If you're willing to take a risk, turn on Fugitive Mode.", mpg123_errors.MPG123_ERR_READER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                if (scan)
                {
                    // We need to scan the file to get accurate duration
                    int scanStatus = NativeStatus.mpg123_scan(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException("Can't scan file for length information", mpg123_errors.MPG123_ERR);
                }

                // Get the actual length
                length = NativeStatus.mpg123_length(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the length of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }

        public static TimeSpan GetDurationSpan(bool scan)
        {
            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo();

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetDuration(scan);
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
