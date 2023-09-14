
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Native.Runtime;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Init;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback;
using System;
using BassBoom.Native.Interop.Output;
using System.Diagnostics;
using BassBoom.Native.Interop.LowLevel;
using System.Runtime.InteropServices;
using System.Text;

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
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing && !InitBasolia._fugitive)
                throw new BasoliaException("Trying to get the duration during playback causes playback corruption! Don't call this function during playback. If you're willing to take a risk, turn on Fugitive Mode.", mpg123_errors.MPG123_ERR_READER);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;
                if (scan)
                {
                    lock (PlaybackPositioningTools.PositionLock)
                    {
                        // We need to scan the file to get accurate duration
                        int scanStatus = NativeStatus.mpg123_scan(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException("Can't scan file for length information", mpg123_errors.MPG123_ERR);
                    }
                }

                // Get the actual length
                length = NativeStatus.mpg123_length(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't determine the length of the file", mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }

        public static TimeSpan GetDurationSpan(bool scan)
        {
            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo();

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetDuration(scan);
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan GetDurationSpanFromSamples(int samples)
        {
            // First, get the format information
            var formatInfo = FormatTools.GetFormatInfo();

            // Get the required values
            long rate = formatInfo.rate;
            long seconds = samples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        public static int GetFrameSize()
        {
            int frameSize;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var outHandle = Mpg123Instance._out123Handle;

                // Get the output format to get the frame size
                int getStatus = NativeOutputLib.out123_getformat(outHandle, null, null, null, out frameSize);
                if (getStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException($"Can't get the output.", (out123_error)getStatus);
                Debug.WriteLine($"Got frame size {frameSize}");
            }
            return frameSize;
        }

        public static int GetBufferSize()
        {
            int bufferSize;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Now, buffer the entire music file and create an empty array based on its size
                bufferSize = NativeLowIo.mpg123_outblock(handle);
                Debug.WriteLine($"Buffer size is {bufferSize}");
            }
            return bufferSize;
        }

        public static void GetId3Metadata(out Id3V1Metadata managedV1, out Id3V2Metadata managedV2)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            IntPtr v1 = IntPtr.Zero;
            IntPtr v2 = IntPtr.Zero;
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // We need to scan the file to get accurate info
                int scanStatus = NativeStatus.mpg123_scan(handle);
                if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't scan file for metadata information", mpg123_errors.MPG123_ERR);

                // Now, get the metadata info.
                int getStatus = NativeMetadata.mpg123_id3(handle, ref v1, ref v2);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't get metadata information", (mpg123_errors)getStatus);
            }

            // Check the pointers before trying to get metadata
            managedV1 = new();
            managedV2 = new();
            if (v1 != IntPtr.Zero)
            {
                // v1 is not NULL, so construct a managed v1 metadata.
                var nativeV1 = Marshal.PtrToStructure<mpg123_id3v1>(v1);
                Debug.WriteLine("Created v1 structure at memory address 0x{0:X16}", v1);

                // First, make a new instance of the managed V1 metadata
                string tag = new(nativeV1.tag);
                string title = new(nativeV1.title);
                string album = new(nativeV1.album);
                string artist = new(nativeV1.artist);
                string year = new(nativeV1.year);
                string comment = new(nativeV1.comment);
                int genreIdx = nativeV1.genre;

                // Then, install the instance.
                var managedV1Instance = new Id3V1Metadata(tag, title, artist, album, year, comment, genreIdx);
                managedV1 = managedV1Instance;
            }
            if (v2 != IntPtr.Zero)
            {
                // v2 is not NULL, so construct a managed v1 metadata.
                var nativeV2 = Marshal.PtrToStructure<mpg123_id3v2>(v2);
                Debug.WriteLine("Created v2 structure at memory address 0x{0:X16}", v2);

                // First, make a new instance of the managed V2 metadata
                unsafe
                {
                    // Checking for NULLs is necessary before trying to set the values.
                    string title = new nint(nativeV2.title) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.title->p), nativeV2.title->size - 1) :
                        "";
                    string artist = new nint(nativeV2.artist) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.artist->p), nativeV2.artist->size - 1) :
                        "";
                    string album = new nint(nativeV2.album) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.album->p), nativeV2.album->size - 1) :
                        "";
                    string year = new nint(nativeV2.year) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.year->p), nativeV2.year->size - 1) :
                        "";
                    string genre = new nint(nativeV2.genre) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.genre->p), nativeV2.genre->size - 1) :
                        "";
                    string comment = new nint(nativeV2.comment) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new nint(nativeV2.comment->p), nativeV2.comment->size - 1) :
                        "";

                    // TODO: Deal with the lists
                    var managedV2Instance = new Id3V2Metadata(title, artist, album, year, comment, genre,
                        Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
                    managedV2 = managedV2Instance;
                }
            }
        }

        public static string GetIcyMetadata()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            string icy = "";
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // We need to scan the file to get accurate info
                int scanStatus = NativeStatus.mpg123_scan(handle);
                if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't scan file for metadata information", mpg123_errors.MPG123_ERR);

                // Now, get the metadata info.
                int getStatus = NativeMetadata.mpg123_icy(handle, ref icy);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't get metadata information", (mpg123_errors)getStatus);
            }
            return icy;
        }

        public static FrameInfo GetFrameInfo()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            mpg123_frameinfo frameInfo = default;
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // We need to scan the file to get accurate info
                int scanStatus = NativeStatus.mpg123_scan(handle);
                if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't scan file for frame information", mpg123_errors.MPG123_ERR);

                // Now, get the frame info.
                int getStatus = NativeStatus.mpg123_info(handle, ref frameInfo);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't get frame information", (mpg123_errors)getStatus);
            }

            // Move every info to the class
            mpg123_version version = frameInfo.version;
            int layer = frameInfo.layer;
            long rate = frameInfo.rate;
            mpg123_mode mode = frameInfo.mode;
            int mode_ext = frameInfo.mode_ext;
            int framesize = frameInfo.framesize;
            mpg123_flags flags = frameInfo.flags;
            int emphasis = frameInfo.emphasis;
            int bitrate = frameInfo.bitrate;
            int abr_rate = frameInfo.abr_rate;
            mpg123_vbr vbr = frameInfo.vbr;
            var frameInfoInstance = new FrameInfo(version, layer, rate, mode, mode_ext, framesize, flags, emphasis, bitrate, abr_rate, vbr);
            return frameInfoInstance;
        }
    }
}
