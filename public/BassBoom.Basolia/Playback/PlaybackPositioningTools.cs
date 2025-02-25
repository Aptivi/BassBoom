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
using BassBoom.Basolia.Format;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Interop.Output;
using System;
using System.Threading;
using BassBoom.Basolia.Lyrics;
using BassBoom.Native;
using BassBoom.Basolia.Exceptions;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback positioning tools
    /// </summary>
    public static class PlaybackPositioningTools
    {
        internal static object PositionLock = new();

        /// <summary>
        /// Gets the current duration of the file (samples)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Current duration in samples</returns>
        public static int GetCurrentDuration(BasoliaMedia? basolia)
        {
            int length;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't play a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the length
                var @delegate = NativeInitializer.GetDelegate<NativePositioning.mpg123_tell>(NativeInitializer.libManagerMpv, nameof(NativePositioning.mpg123_tell));
                length = @delegate.Invoke(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the current duration of the file", mpg123_errors.MPG123_ERR);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo(basolia);

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetCurrentDuration(basolia);
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
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
                    throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);

                // TODO: Unstub this function
            }
        }

        /// <summary>
        /// Seeks to a specific frame
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="frame">An MPEG frame number</param>
        public static void SeekToFrame(BasoliaMedia? basolia, int frame)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
                if (basolia is null)
                    throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);

                // TODO: Unstub this function
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
                    throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);
                if (lyricLine is null)
                    throw new BasoliaException("Lyric line is not provided to seek to", mpg123_errors.MPG123_BAD_FILE);

                // Get the length, convert it to frames, and seek
                var length = lyricLine.LineSpan.TotalSeconds;
                int frame = (int)(length * FormatTools.GetFormatInfo(basolia).rate);
                SeekToFrame(basolia, frame);
            }
        }

        /// <summary>
        /// Drops all MPEG frames to the device
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void Drop(BasoliaMedia? basolia)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
                if (basolia is null)
                    throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

                // Check to see if the file is open
                if (!FileTools.IsOpened(basolia))
                    throw new BasoliaException("Can't drop.", mpg123_errors.MPG123_BAD_FILE);

                // TODO: Unstub this function
            }
        }
    }
}
