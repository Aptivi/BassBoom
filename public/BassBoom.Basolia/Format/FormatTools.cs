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
using Textify.General;

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
                var handle = basolia._mpg123Handle;

                // Get the rate, the number of channels, and encoding
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_getformat>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_getformat));
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
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_rates>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_rates));
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
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_encodings>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_encodings));
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
                throw new BasoliaException("Encoding {0} not found.".FormatString(encoding), mpg123_errors.MPG123_BAD_TYPES);

            // We're now entering the dangerous zone
            unsafe
            {
                // Get the encoding name
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_enc_name>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_enc_name));
                IntPtr namePtr = @delegate.Invoke(encoding);
                encodingName = Marshal.PtrToStringAnsi(namePtr);
            }

            // We're now entering the safe zone
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
                throw new BasoliaException("Encoding {0} not found.".FormatString(encoding), mpg123_errors.MPG123_BAD_TYPES);

            // We're now entering the dangerous zone
            unsafe
            {
                // Get the encoding description
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_enc_longname>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_enc_longname));
                IntPtr descriptionPtr = @delegate.Invoke(encoding);
                encodingDescription = Marshal.PtrToStringAnsi(descriptionPtr);
            }

            // We're now entering the safe zone
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

            // We're now entering the dangerous zone
            int getStatus;
            nint fmtlist = IntPtr.Zero;
            unsafe
            {
                var outHandle = basolia._out123Handle;

                // Get the list of supported formats
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_formats>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_formats));
                getStatus = @delegate.Invoke(outHandle, IntPtr.Zero, 0, 0, 0, ref fmtlist);
                if (getStatus == (int)out123_error.OUT123_ERR)
                    throw new BasoliaOutException("Can't get format information", (out123_error)getStatus);
            }

            // Now, iterate through the list of supported formats
            for (int i = 0; i < getStatus; i++)
            {
                long rate;
                int channels, encoding;

                // The "long" rate is different on our Windows compilations than on Linux compilations.
                if (PlatformHelper.IsOnWindows() || !Environment.Is64BitOperatingSystem)
                {
                    var fmtStruct = Marshal.PtrToStructure<mpg123_fmt_win>(fmtlist);
                    rate = fmtStruct.rate;
                    channels = fmtStruct.channels;
                    encoding = fmtStruct.encoding;
                }
                else
                {
                    var fmtStruct = Marshal.PtrToStructure<mpg123_fmt>(fmtlist);
                    rate = fmtStruct.rate;
                    channels = fmtStruct.channels;
                    encoding = fmtStruct.encoding;
                }

                // Check the validity of the three values
                if (rate >= 0 && channels >= 0 && encoding >= 0)
                {
                    var fmtInstance = new FormatInfo(rate, channels, encoding);
                    formats.Add(fmtInstance);
                }
            }

            // We're now entering the safe zone
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
                var handle = basolia._mpg123Handle;

                // Check for support
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_format_support>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format_support));
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
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_encsize>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_encsize));
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
                var handle = basolia._mpg123Handle;

                // Check for support
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_format_none>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format_none));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't reset output encoding", (mpg123_errors)resetStatus);
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
                var handle = basolia._mpg123Handle;

                // Check for support
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_format_all>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format_all));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't set output format", (mpg123_errors)resetStatus);
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
                var handle = basolia._mpg123Handle;

                // Check for support
                var delegate2 = MpgNative.GetDelegate<NativeOutput.mpg123_format>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format));
                int formatStatus = delegate2.Invoke(handle, rate, (int)channels, encoding);
                if (formatStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't set output encoding to:" + $" {rate}, {channels}, {encoding}", (mpg123_errors)formatStatus);
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
