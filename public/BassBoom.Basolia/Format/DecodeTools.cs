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

using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Helpers;
using BassBoom.Native;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Decoder tools
    /// </summary>
    public static class DecodeTools
    {
        /// <summary>
        /// Decodes next MPEG frame to internal buffer or reads a frame and returns after setting a new format.
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="num">Frame offset</param>
        /// <param name="audio">Array of decoded audio bytes</param>
        /// <param name="bytes">Number of bytes to read</param>
        /// <returns>MPG123_OK on success.</returns>
        public static int DecodeFrame(BasoliaMedia? basolia, ref int num, ref byte[]? audio, ref int bytes)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't decode the frame of a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the frame
                IntPtr numPtr, bytesPtr, audioPtr = IntPtr.Zero;
                numPtr = new IntPtr(num);
                bytesPtr = new IntPtr(bytes);
                var @delegate = NativeInitializer.GetDelegate<NativeInput.mpg123_decode_frame>(NativeInitializer.libManagerMpv, nameof(NativeInput.mpg123_decode_frame));
                int decodeStatus = @delegate.Invoke(handle, ref numPtr, ref audioPtr, ref bytesPtr);
                num = numPtr.ToInt32();
                bytes = bytesPtr.ToInt32();
                audio = new byte[bytes];
                if (audioPtr != IntPtr.Zero)
                    Marshal.Copy(audioPtr, audio, 0, bytes);
                if (decodeStatus != (int)mpg123_errors.MPG123_OK &&
                    decodeStatus != (int)mpg123_errors.MPG123_NEW_FORMAT &&
                    decodeStatus != (int)mpg123_errors.MPG123_NEED_MORE &&
                    decodeStatus != (int)mpg123_errors.MPG123_DONE)
                    throw new BasoliaException("Can't decode frame", (mpg123_errors)decodeStatus);

                return decodeStatus;
            }
        }

        /// <summary>
        /// Gets all decoders or the supported decoders
        /// </summary>
        /// <param name="onlySupported">Show only supported decoders</param>
        /// <returns>Either an array of all decoders or an array of only the supported decoders according to the current device and driver.</returns>
        public static string[] GetDecoders(bool onlySupported)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var @delegate = NativeInitializer.GetDelegate<NativeDecoder.mpg123_supported_decoders>(NativeInitializer.libManagerMpv, nameof(NativeDecoder.mpg123_supported_decoders));
                var delegate2 = NativeInitializer.GetDelegate<NativeDecoder.mpg123_decoders>(NativeInitializer.libManagerMpv, nameof(NativeDecoder.mpg123_decoders));
                IntPtr decodersPtr = onlySupported ? @delegate.Invoke() : delegate2.Invoke();
                string[] decoders = ArrayVariantLength.GetStringsUnknownLength(decodersPtr);
                return decoders;
            }
        }

        /// <summary>
        /// Gets the current decoder
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns></returns>
        public static string GetCurrentDecoder(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                var handle = basolia._libmpvHandle;
                var @delegate = NativeInitializer.GetDelegate<NativeDecoder.mpg123_current_decoder>(NativeInitializer.libManagerMpv, nameof(NativeDecoder.mpg123_current_decoder));
                IntPtr decoderPtr = @delegate.Invoke(handle);
                return Marshal.PtrToStringAnsi(decoderPtr);
            }
        }

        /// <summary>
        /// Sets the current decoder
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="decoderName">Decoder name</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetCurrentDecoder(BasoliaMedia? basolia, string decoderName)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // Try to set the equalizer value
            unsafe
            {
                string[] decoders = GetDecoders(false);
                if (!decoders.Contains(decoderName))
                    throw new BasoliaException($"Decoder {decoderName} not found", mpg123_errors.MPG123_BAD_DECODER);
                string[] supportedDecoders = GetDecoders(true);
                if (!supportedDecoders.Contains(decoderName))
                    throw new BasoliaException($"Decoder {decoderName} not supported by your device", mpg123_errors.MPG123_BAD_DECODER);
                var handle = basolia._libmpvHandle;
                var @delegate = NativeInitializer.GetDelegate<NativeDecoder.mpg123_decoder>(NativeInitializer.libManagerMpv, nameof(NativeDecoder.mpg123_decoder));
                int status = @delegate.Invoke(handle, decoderName);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException($"Can't set decoder to {decoderName}", (mpg123_errors)status);
            }
        }
    }
}
