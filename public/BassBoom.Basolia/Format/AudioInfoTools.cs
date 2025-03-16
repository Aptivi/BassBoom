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
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using SpecProbe.Software.Platform;
using BassBoom.Basolia.Enumerations;
using BassBoom.Native;
using BassBoom.Basolia.Exceptions;
using BassBoom.Native.Interop.Enumerations;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Audio information tools
    /// </summary>
    public static class AudioInfoTools
    {
        /// <summary>
        /// Gets the duration of the file in samples
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>Number of samples detected by libmpv. If you want to get seconds, use <see cref="FormatTools.GetFormatInfo"/>'s rate result to divide the samples by it.</returns>
        public static int GetDuration(BasoliaMedia? basolia, bool scan)
        {
            int length = 0;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we're playing
            if (PlaybackTools.IsPlaying(basolia))
                throw new BasoliaException("Trying to get the duration during playback causes playback corruption! Don't call this function during playback.", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Always zero for radio stations
            if (FileTools.IsRadioStation(basolia))
                return 0;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;
                if (scan)
                {
                    lock (PlaybackPositioningTools.PositionLock)
                    {
                        // We need to scan the file to get accurate duration

                        // TODO: Unstub this function
                    }
                }

                // Get the actual length

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            return length;
        }

        /// <summary>
        /// Gets the duration of the file in the time span
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public static TimeSpan GetDurationSpan(BasoliaMedia? basolia, bool scan)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo(basolia);

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetDuration(basolia, scan);
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Gets the duration from the number of samples
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="samples">Number of samples</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public static TimeSpan GetDurationSpanFromSamples(BasoliaMedia? basolia, int samples)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // First, get the format information
            var (rate, _, _) = FormatTools.GetFormatInfo(basolia);
            return GetDurationSpanFromSamples(samples, rate);
        }

        /// <summary>
        /// Gets the duration from the number of samples
        /// </summary>
        /// <param name="samples">Number of samples</param>
        /// <param name="rate">Bit rate</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public static TimeSpan GetDurationSpanFromSamples(int samples, long rate)
        {
            // Get the required values
            long seconds = samples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Gets the frame size from the currently open music file
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>The MPEG frame size</returns>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaException"></exception>
        public static int GetFrameSize(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // TODO: Unstub this function
            return 0;
        }

        /// <summary>
        /// Gets the frame length
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Frame length in samples</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetFrameLength(BasoliaMedia? basolia)
        {
            int getStatus = 0;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the frame length

                // TODO: Unstub this function
                Debug.WriteLine($"Got frame length {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the number of samples per frame
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Number of samples per frame</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetSamplesPerFrame(BasoliaMedia? basolia)
        {
            int getStatus = 0;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the samples per frame

                // TODO: Unstub this function
                Debug.WriteLine($"Got frame spf {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the number of seconds per frame
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Number of seconds per frame</returns>
        /// <exception cref="BasoliaException"></exception>
        public static double GetSecondsPerFrame(BasoliaMedia? basolia)
        {
            double getStatus = 0;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the seconds per frame

                // TODO: Unstub this function
                Debug.WriteLine($"Got frame tpf {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the buffer size from the currently open music file.
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Buffer size</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetBufferSize(BasoliaMedia? basolia)
        {
            int bufferSize = 0;
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Now, buffer the entire music file and create an empty array based on its size

                // TODO: Unstub this function
                Debug.WriteLine($"Buffer size is {bufferSize}");
            }
            return bufferSize;
        }

        /// <summary>
        /// Gets the generic buffer size that is suitable in most cases
        /// </summary>
        /// <returns>Buffer size</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetGenericBufferSize()
        {
            InitBasolia.CheckInited();
            int bufferSize = 0;

            unsafe
            {
                // Get the generic buffer size

                // TODO: Unstub this function
                Debug.WriteLine($"Got buffsize {bufferSize}");
            }
            return bufferSize;
        }

        /// <summary>
        /// Gets the ID3 metadata (v2 and v1)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="managedV1">An output to the managed instance of the ID3 metadata version 1</param>
        /// <param name="managedV2">An output to the managed instance of the ID3 metadata version 2</param>
        /// <exception cref="BasoliaException"></exception>
        public static void GetId3Metadata(BasoliaMedia? basolia, out Id3V1Metadata managedV1, out Id3V2Metadata managedV2)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we're playing
            if (PlaybackTools.IsPlaying(basolia))
                throw new BasoliaException("Trying to get the ID3 metadata during playback causes playback corruption! Don't call this function during playback.", MpvError.MPV_ERROR_INVALID_PARAMETER);

            IntPtr v1 = IntPtr.Zero;
            IntPtr v2 = IntPtr.Zero;
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // We need to scan the file to get accurate info
                if (!FileTools.IsRadioStation(basolia))
                {

                    // TODO: Unstub this function
                }

                // Now, get the metadata info.

                // TODO: Unstub this function
            }

            // Check the pointers before trying to get metadata
            managedV1 = new();
            managedV2 = new();

            // TODO: Unstub this function
        }

        /// <summary>
        /// Gets the ICY metadata
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>A string containing ICY metadata</returns>
        /// <exception cref="BasoliaException"></exception>
        public static string GetIcyMetadata(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we're playing
            if (PlaybackTools.IsPlaying(basolia))
                throw new BasoliaException("Trying to get the ICY metadata during playback causes playback corruption! Don't call this function during playback.", MpvError.MPV_ERROR_INVALID_PARAMETER);

            string icy = "";
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // We need to scan the file to get accurate info
                if (!FileTools.IsRadioStation(basolia))
                {

                    // TODO: Unstub this function
                }

                // Now, get the metadata info.

                // TODO: Unstub this function
            }
            return icy;
        }

        /// <summary>
        /// Gets the frame information
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>An instance of <see cref="FrameInfo"/> containing MPEG frame information about the music file</returns>
        /// <exception cref="BasoliaException"></exception>
        public static FrameInfo GetFrameInfo(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't query a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we're playing
            if (PlaybackTools.IsPlaying(basolia))
                throw new BasoliaException("Trying to get the frame information during playback causes playback corruption! Don't call this function during playback.", MpvError.MPV_ERROR_INVALID_PARAMETER);
            
            // Some variables
            FrameVersion version = 0;
            int layer = 0;
            long rate = 0;
            FrameMode mode = 0;
            int mode_ext = 0;
            int framesize = 0;
            FrameFlags flags = 0;
            int emphasis = 0;
            int bitrate = 0;
            int abr_rate = 0;
            FrameVbr vbr = 0;

            // TODO: Unstub this function
            var frameInfoInstance = new FrameInfo(version, layer, rate, mode, mode_ext, framesize, flags, emphasis, bitrate, abr_rate, vbr);
            return frameInfoInstance;
        }
    }
}
