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
using System.Collections.Generic;
using System.Linq;
using SpecProbe.Platform;
using BassBoom.Basolia.Enumerations;

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
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>Number of samples detected by MPG123. If you want to get seconds, use <see cref="FormatTools.GetFormatInfo"/>'s rate result to divide the samples by it.</returns>
        public static int GetDuration(bool scan)
        {
            int length;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing)
                throw new BasoliaException("Trying to get the duration during playback causes playback corruption! Don't call this function during playback.", mpg123_errors.MPG123_ERR_READER);

            // Always zero for radio stations
            if (FileTools.IsRadioStation)
                return 0;

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

        /// <summary>
        /// Gets the duration of the file in the time span
        /// </summary>
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
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

        /// <summary>
        /// Gets the duration from the number of samples
        /// </summary>
        /// <param name="samples">Number of samples</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public static TimeSpan GetDurationSpanFromSamples(int samples)
        {
            // First, get the format information
            var (rate, _, _) = FormatTools.GetFormatInfo();
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
        /// <returns>The MPEG frame size</returns>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaOutException"></exception>
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

        /// <summary>
        /// Gets the frame length
        /// </summary>
        /// <returns>Frame length in samples</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetFrameLength()
        {
            int getStatus;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the frame length
                getStatus = NativeStatus.mpg123_framelength(handle);
                if (getStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException($"Can't get the frame length.", mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got frame length {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the number of samples per frame
        /// </summary>
        /// <returns>Number of samples per frame</returns>
        /// <exception cref="BasoliaException"></exception>
        public static int GetSamplesPerFrame()
        {
            int getStatus;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // Get the samples per frame
                getStatus = NativeStatus.mpg123_spf(handle);
                if (getStatus < 0)
                    throw new BasoliaException($"Can't get the samples per frame.", mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got frame spf {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the buffer size from the currently open music file.
        /// </summary>
        /// <returns>Buffer size</returns>
        /// <exception cref="BasoliaException"></exception>
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

        /// <summary>
        /// Gets the ID3 metadata (v2 and v1)
        /// </summary>
        /// <param name="managedV1">An output to the managed instance of the ID3 metadata version 1</param>
        /// <param name="managedV2">An output to the managed instance of the ID3 metadata version 2</param>
        /// <exception cref="BasoliaException"></exception>
        public static void GetId3Metadata(out Id3V1Metadata managedV1, out Id3V2Metadata managedV2)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing)
                throw new BasoliaException("Trying to get the ID3 metadata during playback causes playback corruption! Don't call this function during playback.", mpg123_errors.MPG123_ERR_READER);

            IntPtr v1 = IntPtr.Zero;
            IntPtr v2 = IntPtr.Zero;
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // We need to scan the file to get accurate info
                if (!FileTools.IsRadioStation)
                {
                    int scanStatus = NativeStatus.mpg123_scan(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException("Can't scan file for frame information", mpg123_errors.MPG123_ERR);
                }

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
                    string title = new IntPtr(nativeV2.title) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.title->p), nativeV2.title->size.ToInt32() - 1) :
                        "";
                    string artist = new IntPtr(nativeV2.artist) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.artist->p), nativeV2.artist->size.ToInt32() - 1) :
                        "";
                    string album = new IntPtr(nativeV2.album) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.album->p), nativeV2.album->size.ToInt32() - 1) :
                        "";
                    string year = new IntPtr(nativeV2.year) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.year->p), nativeV2.year->size.ToInt32() - 1) :
                        "";
                    string genre = new IntPtr(nativeV2.genre) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.genre->p), nativeV2.genre->size.ToInt32() - 1) :
                        "";
                    string comment = new IntPtr(nativeV2.comment) != IntPtr.Zero ?
                        Marshal.PtrToStringAnsi(new IntPtr(nativeV2.comment->p), nativeV2.comment->size.ToInt32() - 1) :
                        "";

                    // Comments...
                    var commentsSize = Marshal.SizeOf(typeof(mpg123_text));
                    var commentsList = new List<mpg123_text>();
                    var commentsListManaged = new List<(string, string)>();
                    var commentsPtr = nativeV2.comment_list;
                    for (int i = 0; i < nativeV2.comments; i++)
                    {
                        commentsList.Add(Marshal.PtrToStructure<mpg123_text>(commentsPtr));
                        commentsPtr += commentsSize;
                    }
                    commentsListManaged.AddRange(commentsList.Select((text) =>
                        (new IntPtr(text.description.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.description.p)) : "",
                         new IntPtr(text.text.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.text.p)) : ""))
                    );

                    // Texts...
                    var textsSize = Marshal.SizeOf(typeof(mpg123_text));
                    var textsList = new List<mpg123_text>();
                    var textsListManaged = new List<(string, string)>();
                    var textsPtr = nativeV2.text;
                    for (int i = 0; i < nativeV2.texts; i++)
                    {
                        textsList.Add(Marshal.PtrToStructure<mpg123_text>(textsPtr));
                        textsPtr += textsSize;
                    }
                    textsListManaged.AddRange(textsList.Select((text) =>
                        (new IntPtr(text.description.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.description.p)) : "",
                         new IntPtr(text.text.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.text.p)) : ""))
                    );

                    // Extras...
                    var extrasSize = Marshal.SizeOf(typeof(mpg123_text));
                    var extrasList = new List<mpg123_text>();
                    var extrasListManaged = new List<(string, string)>();
                    var extrasPtr = nativeV2.extra;
                    for (int i = 0; i < nativeV2.extras; i++)
                    {
                        extrasList.Add(Marshal.PtrToStructure<mpg123_text>(extrasPtr));
                        extrasPtr += extrasSize;
                    }
                    extrasListManaged.AddRange(extrasList.Select((text) =>
                        (new IntPtr(text.description.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.description.p)) : "",
                         new IntPtr(text.text.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(text.text.p)) : ""))
                    );

                    // Pictures...
                    var pictureSize = Marshal.SizeOf(typeof(mpg123_picture));
                    var pictureList = new List<mpg123_picture>();
                    var pictureListManaged = new List<(string, string)>();
                    var picturePtr = nativeV2.picture;
                    for (int i = 0; i < nativeV2.pictures; i++)
                    {
                        pictureList.Add(Marshal.PtrToStructure<mpg123_picture>(picturePtr));
                        picturePtr += pictureSize;
                    }
                    extrasListManaged.AddRange(pictureList.Select((picture) =>
                        (new IntPtr(picture.description.p) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(picture.description.p)) : "",
                         new IntPtr(picture.data) != IntPtr.Zero ? Marshal.PtrToStringAnsi(new IntPtr(picture.data)) : ""))
                    );
                    var managedV2Instance = new Id3V2Metadata(title, artist, album, year, comment, genre,
                        [.. commentsListManaged], [.. textsListManaged], [.. extrasListManaged], [.. extrasListManaged]);
                    managedV2 = managedV2Instance;
                }
            }
        }

        /// <summary>
        /// Gets the ICY metadata
        /// </summary>
        /// <returns>A string containing ICY metadata</returns>
        /// <exception cref="BasoliaException"></exception>
        public static string GetIcyMetadata()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing)
                throw new BasoliaException("Trying to get the ICY metadata during playback causes playback corruption! Don't call this function during playback.", mpg123_errors.MPG123_ERR_READER);

            string icy = "";
            unsafe
            {
                var handle = Mpg123Instance._mpg123Handle;

                // We need to scan the file to get accurate info
                if (!FileTools.IsRadioStation)
                {
                    int scanStatus = NativeStatus.mpg123_scan(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException("Can't scan file for frame information", mpg123_errors.MPG123_ERR);
                }

                // Now, get the metadata info.
                int getStatus = NativeMetadata.mpg123_icy(handle, ref icy);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException("Can't get metadata information", (mpg123_errors)getStatus);
            }
            return icy;
        }

        /// <summary>
        /// Gets the frame information
        /// </summary>
        /// <returns>An instance of <see cref="FrameInfo"/> containing MPEG frame information about the music file</returns>
        /// <exception cref="BasoliaException"></exception>
        public static FrameInfo GetFrameInfo()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!FileTools.IsOpened)
                throw new BasoliaException("Can't query a file that's not open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (PlaybackTools.Playing)
                throw new BasoliaException("Trying to get the frame information during playback causes playback corruption! Don't call this function during playback.", mpg123_errors.MPG123_ERR_READER);
            
            // Some variables
            FrameVersion version;
            int layer;
            long rate;
            FrameMode mode;
            int mode_ext;
            int framesize;
            FrameFlags flags;
            int emphasis;
            int bitrate;
            int abr_rate;
            FrameVbr vbr;

            // In Windows, the "long" rate byte number differs from the Linux version.
            if (PlatformHelper.IsOnWindows() || !Environment.Is64BitOperatingSystem)
            {
                mpg123_frameinfo_win frameInfo = default;
                unsafe
                {
                    var handle = Mpg123Instance._mpg123Handle;

                    // We need to scan the file to get accurate info, but it only works with files
                    if (!FileTools.IsRadioStation)
                    {
                        int scanStatus = NativeStatus.mpg123_scan(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException("Can't scan file for frame information", mpg123_errors.MPG123_ERR);
                    }

                    // Now, get the frame info.
                    int getStatus = NativeStatus.mpg123_info_win(handle, ref frameInfo);
                    if (getStatus != (int)mpg123_errors.MPG123_OK)
                        throw new BasoliaException("Can't get frame information", (mpg123_errors)getStatus);
                }

                // Move every info to the class
                version = (FrameVersion)frameInfo.version;
                layer = frameInfo.layer;
                rate = frameInfo.rate;
                mode = (FrameMode)frameInfo.mode;
                mode_ext = frameInfo.mode_ext;
                framesize = frameInfo.framesize;
                flags = (FrameFlags)frameInfo.flags;
                emphasis = frameInfo.emphasis;
                bitrate = frameInfo.bitrate;
                abr_rate = frameInfo.abr_rate;
                vbr = (FrameVbr)frameInfo.vbr;
            }
            else
            {
                mpg123_frameinfo frameInfo = default;
                unsafe
                {
                    var handle = Mpg123Instance._mpg123Handle;

                    // We need to scan the file to get accurate info
                    if (!FileTools.IsRadioStation)
                    {
                        int scanStatus = NativeStatus.mpg123_scan(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException("Can't scan file for frame information", mpg123_errors.MPG123_ERR);
                    }

                    // Now, get the frame info.
                    int getStatus = NativeStatus.mpg123_info(handle, ref frameInfo);
                    if (getStatus != (int)mpg123_errors.MPG123_OK)
                        throw new BasoliaException("Can't get frame information", (mpg123_errors)getStatus);
                }

                // Move every info to the class
                version = (FrameVersion)frameInfo.version;
                layer = frameInfo.layer;
                rate = frameInfo.rate;
                mode = (FrameMode)frameInfo.mode;
                mode_ext = frameInfo.mode_ext;
                framesize = frameInfo.framesize;
                flags = (FrameFlags)frameInfo.flags;
                emphasis = frameInfo.emphasis;
                bitrate = frameInfo.bitrate;
                abr_rate = frameInfo.abr_rate;
                vbr = (FrameVbr)frameInfo.vbr;
            }
            var frameInfoInstance = new FrameInfo(version, layer, rate, mode, mode_ext, framesize, flags, emphasis, bitrate, abr_rate, vbr);
            return frameInfoInstance;
        }
    }
}
