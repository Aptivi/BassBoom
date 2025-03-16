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
using BassBoom.Native.Interop.Enumerations;
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
        /// <returns>libmpv_OK on success.</returns>
        public static int DecodeFrame(BasoliaMedia? basolia, ref int num, ref byte[]? audio, ref int bytes)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't decode the frame of a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Get the frame
                IntPtr numPtr, bytesPtr, audioPtr = IntPtr.Zero;
                numPtr = new IntPtr(num);
                bytesPtr = new IntPtr(bytes);

                // TODO: Unstub this function
                int decodeStatus = 0;
                num = numPtr.ToInt32();
                bytes = bytesPtr.ToInt32();
                audio = new byte[bytes];
                if (audioPtr != IntPtr.Zero)
                    Marshal.Copy(audioPtr, audio, 0, bytes);
                if (decodeStatus != (int)MpvError.MPV_ERROR_SUCCESS &&
                    decodeStatus != (int)MpvError.MPV_ERROR_NOTHING_TO_PLAY)
                    throw new BasoliaException("Can't decode frame", (MpvError)decodeStatus);

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

            // Try to get the decoders
            unsafe
            {

                // TODO: Unstub this function
            }
            return [];
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
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Try to get the curent decoder
            unsafe
            {

                // TODO: Unstub this function
            }
            return "";
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
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Try to set the equalizer value
            unsafe
            {
                string[] decoders = GetDecoders(false);
                if (!decoders.Contains(decoderName))
                    throw new BasoliaException($"Decoder {decoderName} not found", MpvError.MPV_ERROR_INVALID_PARAMETER);
                string[] supportedDecoders = GetDecoders(true);
                if (!supportedDecoders.Contains(decoderName))
                    throw new BasoliaException($"Decoder {decoderName} not supported by your device", MpvError.MPV_ERROR_INVALID_PARAMETER);
                var handle = basolia._libmpvHandle;

                // TODO: Unstub this function
            }
        }
    }
}
