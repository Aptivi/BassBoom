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

using System;
using System.Collections.Generic;
using SpecProbe.Software.Platform;
using BassBoom.Native;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Helpers;
using System.Linq;
using BassBoom.Native.Interop.Enumerations;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Audio format tools
    /// </summary>
    public static class FormatTools
    {
        /// <summary>
        /// Gets the format information
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static (long rate, int channels, int encoding) GetFormatInfo(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            long fileRate = 0;
            int fileChannel = 0, fileEncoding = 0;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the rate, the number of channels, and encoding

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            return (fileRate, fileChannel, fileEncoding);
        }
        
        /// <summary>
        /// Gets the rate list supported by the library
        /// </summary>
        public static int[] GetRates()
        {
            InitBasolia.CheckInited();
            int[] rates = [];

            // We're now entering the dangerous zone
            unsafe
            {
                // Get the rates

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            return rates;
        }
        
        /// <summary>
        /// Gets the encoding list supported by the library
        /// </summary>
        public static int[] GetEncodings()
        {
            InitBasolia.CheckInited();
            int[] encodings = [];

            // We're now entering the dangerous zone
            unsafe
            {
                // Get the encodings

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            return encodings;
        }

        /// <summary>
        /// Gets the encoding name
        /// </summary>
        /// <param name="encoding">Encoding ID</param>
        /// <returns>Name of the encoding in short form</returns>
        public static string GetEncodingName(int encoding)
        {
            InitBasolia.CheckInited();
            string encodingName = "";

            // Check the encoding
            int[] encodings = GetEncodings();
            if (!encodings.Contains(encoding))
                throw new BasoliaException($"Encoding {encoding} not found.", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // TODO: Unstub this function
            return encodingName;
        }

        /// <summary>
        /// Gets the encoding description
        /// </summary>
        /// <param name="encoding">Encoding ID</param>
        /// <returns>Description of the encoding in short form</returns>
        public static string GetEncodingDescription(int encoding)
        {
            InitBasolia.CheckInited();
            string encodingDescription = "";

            // Check the encoding
            int[] encodings = GetEncodings();
            if (!encodings.Contains(encoding))
                throw new BasoliaException($"Encoding {encoding} not found.", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // TODO: Unstub this function
            return encodingDescription;
        }

        /// <summary>
        /// Gets the supported formats
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static FormatInfo[] GetFormats(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            var formats = new List<FormatInfo>();

            // TODO: Unstub this function
            return [.. formats];
        }

        /// <summary>
        /// Is this format supported?
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="rate">Rate</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="channelCount">Mono, stereo, or both?</param>
        public static bool IsFormatSupported(BasoliaMedia? basolia, long rate, int encoding, out ChannelCount channelCount)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            channelCount = ChannelCount.Unknown;
            return channelCount != ChannelCount.Unknown;
        }

        /// <summary>
        /// Is this format supported?
        /// </summary>
        /// <param name="encoding">Encoding</param>
        public static int GetEncodingSize(int encoding)
        {
            InitBasolia.CheckInited();
            int size = -1;

            // We're now entering the dangerous zone
            unsafe
            {
                // Check for support

                // TODO: Unstub this function
            }

            // We're now entering the safe zone
            return size;
        }

        /// <summary>
        /// Makes the underlying media handler accept no format
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static void NoFormat(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support

                // TODO: Unstub this function
            }
        }

        /// <summary>
        /// Makes the underlying media handler accept all formats
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static void AllFormats(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support

                // TODO: Unstub this function
            }
        }

        /// <summary>
        /// Makes the underlying media handler use this specific format
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="rate">Rate</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="channels">Mono, stereo, or both?</param>
        public static void UseFormat(BasoliaMedia? basolia, long rate, ChannelCount channels, int encoding)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support

                // TODO: Unstub this function
            }
        }
    }
}
