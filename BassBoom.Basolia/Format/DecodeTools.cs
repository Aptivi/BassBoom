using BassBoom.Basolia.File;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using BassBoom.Native.Runtime;
using System;
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
        /// <param name="num">Frame offset</param>
        /// <param name="audio">Array of decoded audio bytes</param>
        /// <param name="bytes">Number of bytes to read</param>
        /// <returns>MPG123_OK on success.</returns>
        public static int DecodeFrame(ref int num, ref byte[] audio, ref int bytes)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't decode the frame of a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the frame
                IntPtr numPtr, bytesPtr, audioPtr = IntPtr.Zero;
                numPtr = new IntPtr(num);
                bytesPtr = new IntPtr(bytes);
                int decodeStatus = NativeInput.mpg123_decode_frame(handle, ref numPtr, ref audioPtr, ref bytesPtr);
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
    }
}
