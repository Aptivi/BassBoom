using BassBoom.Native.Runtime;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BassBoom.Native.Interop.Init;
using BassBoom.Basolia.File;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// Audio information tools
    /// </summary>
    public static class AudioInfoTools
    {
        /// <summary>
        /// Gets the duration of the file
        /// </summary>
        public static int GetDuration(bool scan)
        {
            int length;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                if (scan)
                {
                    // We need to scan the file to get accurate duration
                    int scanStatus = NativeStatus.mpg123_scan(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException("Can't scan file for length information", mpg123_errors.MPG123_ERR);
                }

                // Get the actual length
                length = NativeStatus.mpg123_length(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the length of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }
    }
}
