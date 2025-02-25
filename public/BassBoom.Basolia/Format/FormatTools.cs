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

using BassBoom.Native.Interop.Play;
using BassBoom.Native.Interop.Init;
using System;
using BassBoom.Native.Interop.Output;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BassBoom.Native.Interop.Analysis;
using SpecProbe.Software.Platform;
using BassBoom.Native;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Helpers;
using System.Linq;

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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            long fileRate;
            int fileChannel, fileEncoding;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the rate, the number of channels, and encoding
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_getformat>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_getformat));
                int length = @delegate.Invoke(handle, out fileRate, out fileChannel, out fileEncoding);
                if (length != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't determine the format of the file", mpg123_errors.MPG123_ERR);
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
            int[] rates;

            // We're now entering the dangerous zone
            unsafe
            {
                // Get the rates
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_rates>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_rates));
                @delegate.Invoke(out IntPtr ratesPtr, out int count);
                rates = ArrayVariantLength.GetIntegersKnownLength(ratesPtr, count, PlatformHelper.IsOnWindows() ? sizeof(int) : sizeof(long));
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
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_encodings>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_encodings));
                @delegate.Invoke(out IntPtr encodingsPtr, out int count);
                encodings = ArrayVariantLength.GetIntegersKnownLength(encodingsPtr, count, sizeof(int));
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
                throw new BasoliaException($"Encoding {encoding} not found.", mpg123_errors.MPG123_BAD_TYPES);

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
                throw new BasoliaException($"Encoding {encoding} not found.", mpg123_errors.MPG123_BAD_TYPES);

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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_format_support>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_format_support));
                int channelCountInt = @delegate.Invoke(handle, rate, encoding);
                channelCount = channelCountInt == 0 ? ChannelCount.Unknown : (ChannelCount)channelCountInt;
            }

            // We're now entering the safe zone
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
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_encsize>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_encsize));
                size = @delegate.Invoke(encoding);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_format_none>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_format_none));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't reset output encoding", (mpg123_errors)resetStatus);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support
                var @delegate = NativeInitializer.GetDelegate<NativeOutput.mpg123_format_all>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_format_all));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set output format", (mpg123_errors)resetStatus);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Check for support
                var delegate2 = NativeInitializer.GetDelegate<NativeOutput.mpg123_format>(NativeInitializer.libManagerMpv, nameof(NativeOutput.mpg123_format));
                int formatStatus = delegate2.Invoke(handle, rate, (int)channels, encoding);
                if (formatStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set output encoding to {rate}, {channels}, {encoding}", (mpg123_errors)formatStatus);
            }
        }

        #region Library-independent functions
        /// <summary>
        /// Gets a PCM sample size for a given encoding (library doesn't need to be initialized)
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <returns>Sample size in bytes</returns>
        public static int GetSampleSize(int encoding)
        {
            int sampleSize = 0;
            var enumEncoding = (mpg123_enc_enum)encoding;
            if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_8))
                sampleSize = 1;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_16))
                sampleSize = 2;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_24))
                sampleSize = 3;
            else if (enumEncoding.HasFlag(mpg123_enc_enum.MPG123_ENC_32) ||
                enumEncoding == mpg123_enc_enum.MPG123_ENC_FLOAT_32)
                sampleSize = 4;
            else if (enumEncoding == mpg123_enc_enum.MPG123_ENC_FLOAT_64)
                sampleSize = 8;
            return sampleSize;
        }

        /// <summary>
        /// Gets a zero sample representation
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <param name="sampleSize">Sample size in bytes. See <see cref="GetSampleSize(int)"/></param>
        /// <param name="lsbOffset">LSB offset in bytes</param>
        /// <returns>Zero sample size in bytes</returns>
        public static int GetZeroSample(int encoding, int sampleSize, int lsbOffset)
        {
            var enumEncoding = (mpg123_enc_enum)encoding;
            if (enumEncoding == mpg123_enc_enum.MPG123_ENC_ULAW_8)
                return lsbOffset == 0 ? 0xff : 0x00;
            else if (enumEncoding == mpg123_enc_enum.MPG123_ENC_ALAW_8)
                return lsbOffset == 0 ? 0xd5 : 0x00;
            else if ((enumEncoding & (mpg123_enc_enum.MPG123_ENC_SIGNED | mpg123_enc_enum.MPG123_ENC_FLOAT)) > 0 ||
                sampleSize != (lsbOffset + 1))
                return 0x00;
            return 0x80;
        }
        #endregion
    }
}
