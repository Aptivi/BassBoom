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
using System;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Exceptions;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Basolia.Helpers;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback positioning tools
    /// </summary>
    public static class PlaybackPositioningTools
    {
        internal static object PositionLock = new();

        /// <summary>
        /// Gets the current duration of the file (seconds)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Current duration in seconds</returns>
        public static long GetCurrentDuration(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't play a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            long length;
            unsafe
            {
                length = MpvPropertyHandler.GetIntegerProperty(basolia, "time-pos/full");
            }

            // We're now entering the safe zone
            return length;
        }

        /// <summary>
        /// Gets the current duration of the file (time span)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>A time span instance that describes the current duration of the file</returns>
        public static TimeSpan GetCurrentDurationSpan(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // First, get the format information
            long duration = GetCurrentDuration(basolia);
            return TimeSpan.FromSeconds(duration);
        }

        /// <summary>
        /// Seeks to the beginning of the music
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static void SeekToTheBeginning(BasoliaMedia? basolia)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
                if (basolia is null)
                    throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Seek to 0 sec
                SeekTo(basolia, 0);
            }
        }

        /// <summary>
        /// Seeks to a specific frame
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="seconds">Duration in seconds in absolute form</param>
        public static void SeekTo(BasoliaMedia? basolia, long seconds)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
                if (basolia is null)
                    throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Seek the file
                MpvCommandHandler.RunCommand(basolia, "seek", seconds.ToString(), "absolute");
            }
        }

        /// <summary>
        /// Seeks according to the lyric line
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="lyricLine">Lyric line instance</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SeekLyric(BasoliaMedia? basolia, LyricLine lyricLine)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
                if (basolia is null)
                    throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);
                if (lyricLine is null)
                    throw new BasoliaException("Lyric line is not provided to seek to", MpvError.MPV_ERROR_INVALID_PARAMETER);

                // Get the length and seek
                long length = (long)lyricLine.LineSpan.TotalSeconds;
                SeekTo(basolia, length);
            }
        }
    }
}
