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

using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Interop.Output;
using System;
using System.Threading;
using BassBoom.Basolia.Lyrics;
using BassBoom.Native;

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
                var handle = MpgNative._mpg123Handle;

                // Get the length
                length = NativePositioning.mpg123_tell(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the current duration of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }

        /// <summary>
        /// Gets the current duration of the file (time span)
        /// </summary>
        /// <returns>A time span instance that describes the current duration of the file</returns>
        public static TimeSpan GetCurrentDurationSpan()
        {
            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo();

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetCurrentDuration();
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Seeks to the beginning of the music
        /// </summary>
        public static void SeekToTheBeginning()
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();

                // Check to see if the file is open
                if (!FileTools.IsOpened)
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var handle = MpgNative._mpg123Handle;
                    var outHandle = MpgNative._out123Handle;

                    // Get the length
                    PlaybackTools.holding = true;
                    while (PlaybackTools.bufferPlaying)
                        Thread.Sleep(1);
                    Drop();
                    int status = NativePositioning.mpg123_seek(handle, 0, 0);
                    PlaybackTools.holding = false;
                    if (status == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException("Can't seek to the beginning of the file", mpg123_errors.MPG123_LSEEK_FAILED);
                }
            }
        }

        /// <summary>
        /// Seeks to a specific frame
        /// </summary>
        /// <param name="frame">An MPEG frame number</param>
        public static void SeekToFrame(int frame)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();

                // Check to see if the file is open
                if (!FileTools.IsOpened)
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var handle = MpgNative._mpg123Handle;
                    var outHandle = MpgNative._out123Handle;

                    // Get the length
                    PlaybackTools.holding = true;
                    while (PlaybackTools.bufferPlaying)
                        Thread.Sleep(1);
                    Drop();
                    int status = NativePositioning.mpg123_seek(handle, frame, 0);
                    PlaybackTools.holding = false;
                    if (status == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException($"Can't seek to frame #{frame} of the file", (mpg123_errors)status);
                }
            }
        }

        /// <summary>
        /// Seeks according to the lyric line
        /// </summary>
        /// <param name="lyricLine">Lyric line instance</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SeekLyric(LyricLine lyricLine)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();

                // Check to see if the file is open
                if (!FileTools.IsOpened)
                    throw new BasoliaException("Can't seek a file that's not open", mpg123_errors.MPG123_BAD_FILE);
                if (lyricLine is null)
                    throw new BasoliaException("Lyric line is not provided to seek to", mpg123_errors.MPG123_BAD_FILE);

                // Get the length, convert it to frames, and seek
                var length = lyricLine.LineSpan.TotalSeconds;
                int frame = (int)(length * FormatTools.GetFormatInfo().rate);
                SeekToFrame(frame);
            }
        }

        /// <summary>
        /// Drops all MPEG frames to the device
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void Drop()
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();

                // Check to see if the file is open
                if (!FileTools.IsOpened)
                    throw new BasoliaException("Can't drop.", mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var outHandle = MpgNative._out123Handle;
                    NativeOutputLib.out123_drop(outHandle);
                }
            }
        }
    }
}
