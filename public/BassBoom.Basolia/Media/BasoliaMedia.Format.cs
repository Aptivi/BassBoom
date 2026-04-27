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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Helpers;
using BassBoom.Basolia.Languages;
using BassBoom.Basolia.Media.Enumerations;
using BassBoom.Basolia.Media.Format;
using BassBoom.Native;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.LowLevel;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Play;
using SpecProbe.Software.Platform;
using Textify.General;

namespace BassBoom.Basolia.Media
{
    /// <summary>
    /// Basolia instance for media manipulation
    /// </summary>
    public partial class BasoliaMedia
    {
        #region Audio info
        /// <summary>
        /// Gets the duration of the file in samples
        /// </summary>
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>Number of samples detected by MPG123. If you want to get seconds, use <see cref="GetFormatInfo"/>'s rate result to divide the samples by it.</returns>
        public int GetDuration(bool scan)
        {
            int length;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (IsPlaying())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DURATIONONPLAYBACK"), mpg123_errors.MPG123_ERR_READER);

            // Always zero for radio stations
            if (IsRadioStation())
                return 0;

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;
                if (scan)
                {
                    lock (PositionLock)
                    {
                        // We need to scan the file to get accurate duration
                        var delegate2 = MpgNative.GetDelegate<NativeStatus.mpg123_scan>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_scan));
                        int scanStatus = delegate2.Invoke(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DURATIONSCANFAILED"), mpg123_errors.MPG123_ERR);
                    }
                }

                // Get the actual length
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_length>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_length));
                length = @delegate.Invoke(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILELENGTHFAILED"), mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }

        /// <summary>
        /// Gets the duration of the file in the time span
        /// </summary>
        /// <param name="scan">Whether to scan the whole music file or not (seeks to the beginning of the music; don't use during playback.</param>
        /// <returns>A <see cref="TimeSpan"/> instance containing the duration in human-readable format</returns>
        public TimeSpan GetDurationSpan(bool scan)
        {
            // First, get the format information
            var formatInfo = GetFormatInfo();

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
        public TimeSpan GetDurationSpanFromSamples(int samples)
        {
            // First, get the format information
            var (rate, _, _) = GetFormatInfo();
            return FormatTools.GetDurationSpanFromSamples(samples, rate);
        }

        /// <summary>
        /// Gets the frame size from the currently open music file
        /// </summary>
        /// <returns>The MPEG frame size</returns>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaOutException"></exception>
        public int GetFrameSize()
        {
            int frameSize;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var outHandle = _out123Handle;

                // Get the output format to get the frame size
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_getformat>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_getformat));
                int getStatus = @delegate.Invoke(outHandle, null, null, null, out frameSize);
                if (getStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_OUTPUTGETFAILED"), (out123_error)getStatus);
                Debug.WriteLine($"Got frame size {frameSize}");
            }
            return frameSize;
        }

        /// <summary>
        /// Gets the frame length
        /// </summary>
        /// <returns>Frame length in samples</returns>
        /// <exception cref="BasoliaException"></exception>
        public int GetFrameLength()
        {
            int getStatus;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = _mpg123Handle;

                // Get the frame length
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_framelength>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_framelength));
                getStatus = @delegate.Invoke(handle);
                if (getStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMELENGETFAILED"), mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got frame length {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the number of samples per frame
        /// </summary>
        /// <returns>Number of samples per frame</returns>
        /// <exception cref="BasoliaException"></exception>
        public int GetSamplesPerFrame()
        {
            int getStatus;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = _mpg123Handle;

                // Get the samples per frame
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_spf>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_spf));
                getStatus = @delegate.Invoke(handle);
                if (getStatus < 0)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_SPFGETFAILED"), mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got frame spf {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the number of seconds per frame
        /// </summary>
        /// <returns>Number of seconds per frame</returns>
        /// <exception cref="BasoliaException"></exception>
        public double GetSecondsPerFrame()
        {
            double getStatus;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = _mpg123Handle;

                // Get the seconds per frame
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_tpf>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_tpf));
                getStatus = @delegate.Invoke(handle);
                if (getStatus < 0)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_TPFGETFAILED"), mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got frame tpf {getStatus}");
            }
            return getStatus;
        }

        /// <summary>
        /// Gets the buffer size from the currently open music file.
        /// </summary>
        /// <returns>Buffer size</returns>
        /// <exception cref="BasoliaException"></exception>
        public int GetBufferSize()
        {
            int bufferSize;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            unsafe
            {
                var handle = _mpg123Handle;

                // Now, buffer the entire music file and create an empty array based on its size
                var @delegate = MpgNative.GetDelegate<NativeLowIo.mpg123_outblock>(MpgNative.libManagerMpg, nameof(NativeLowIo.mpg123_outblock));
                bufferSize = @delegate.Invoke(handle);
                Debug.WriteLine($"Buffer size is {bufferSize}");
            }
            return bufferSize;
        }

        /// <summary>
        /// Gets the generic buffer size that is suitable in most cases
        /// </summary>
        /// <returns>Buffer size</returns>
        /// <exception cref="BasoliaException"></exception>
        public int GetGenericBufferSize()
        {
            InitBasolia.CheckInited();
            int bufferSize;

            unsafe
            {
                // Get the generic buffer size
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_safe_buffer>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_safe_buffer));
                bufferSize = @delegate.Invoke();
                if (bufferSize < 0)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_GENERICBUFFSIZEGETFAILED"), mpg123_errors.MPG123_ERR);
                Debug.WriteLine($"Got buffsize {bufferSize}");
            }
            return bufferSize;
        }

        /// <summary>
        /// Gets the ID3 metadata (v2 and v1)
        /// </summary>
        /// <param name="managedV1">An output to the managed instance of the ID3 metadata version 1</param>
        /// <param name="managedV2">An output to the managed instance of the ID3 metadata version 2</param>
        /// <exception cref="BasoliaException"></exception>
        public void GetId3Metadata(out Id3V1Metadata managedV1, out Id3V2Metadata managedV2)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (IsPlaying())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_ID3ONPLAYBACK"), mpg123_errors.MPG123_ERR_READER);

            IntPtr v1 = IntPtr.Zero;
            IntPtr v2 = IntPtr.Zero;
            unsafe
            {
                var handle = _mpg123Handle;

                // We need to scan the file to get accurate info
                if (!IsRadioStation())
                {
                    var delegate2 = MpgNative.GetDelegate<NativeStatus.mpg123_scan>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_scan));
                    int scanStatus = delegate2.Invoke(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMESCANFAILED"), mpg123_errors.MPG123_ERR);
                }

                // Now, get the metadata info.
                var @delegate = MpgNative.GetDelegate<NativeMetadata.mpg123_id3>(MpgNative.libManagerMpg, nameof(NativeMetadata.mpg123_id3));
                int getStatus = @delegate.Invoke(handle, ref v1, ref v2);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_METADATAGETFAILED"), (mpg123_errors)getStatus);
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
        public string GetIcyMetadata()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (IsPlaying())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_ICYONPLAYBACK"), mpg123_errors.MPG123_ERR_READER);

            string icy = "";
            unsafe
            {
                var handle = _mpg123Handle;

                // We need to scan the file to get accurate info
                if (!IsRadioStation())
                {
                    var delegate2 = MpgNative.GetDelegate<NativeStatus.mpg123_scan>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_scan));
                    int scanStatus = delegate2.Invoke(handle);
                    if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMESCANFAILED"), mpg123_errors.MPG123_ERR);
                }

                // Now, get the metadata info.
                var @delegate = MpgNative.GetDelegate<NativeMetadata.mpg123_icy>(MpgNative.libManagerMpg, nameof(NativeMetadata.mpg123_icy));
                int getStatus = @delegate.Invoke(handle, ref icy);
                if (getStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_METADATAGETFAILED"), (mpg123_errors)getStatus);
            }
            return icy;
        }

        /// <summary>
        /// Gets the frame information
        /// </summary>
        /// <returns>An instance of <see cref="FrameInfo"/> containing MPEG frame information about the music file</returns>
        /// <exception cref="BasoliaException"></exception>
        public FrameInfo GetFrameInfo()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we're playing
            if (IsPlaying())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMEINFOONPLAYBACK"), mpg123_errors.MPG123_ERR_READER);

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
                    var handle = _mpg123Handle;

                    // We need to scan the file to get accurate info, but it only works with files
                    if (!IsRadioStation())
                    {
                        var delegate2 = MpgNative.GetDelegate<NativeStatus.mpg123_scan>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_scan));
                        int scanStatus = delegate2.Invoke(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMESCANFAILED"), mpg123_errors.MPG123_ERR);
                    }

                    // Now, get the frame info.
                    var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_info_win>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_info));
                    int getStatus = @delegate.Invoke(handle, ref frameInfo);
                    if (getStatus != (int)mpg123_errors.MPG123_OK)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMEINFOGETFAILED"), (mpg123_errors)getStatus);
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
                    var handle = _mpg123Handle;

                    // We need to scan the file to get accurate info
                    if (!IsRadioStation())
                    {
                        var delegate2 = MpgNative.GetDelegate<NativeStatus.mpg123_scan>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_scan));
                        int scanStatus = delegate2.Invoke(handle);
                        if (scanStatus == (int)mpg123_errors.MPG123_ERR)
                            throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMESCANFAILED"), mpg123_errors.MPG123_ERR);
                    }

                    // Now, get the frame info.
                    var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_info>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_info));
                    int getStatus = @delegate.Invoke(handle, ref frameInfo);
                    if (getStatus != (int)mpg123_errors.MPG123_OK)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FRAMEINFOGETFAILED"), (mpg123_errors)getStatus);
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
        #endregion

        #region Decode tools
        /// <summary>
        /// Decodes next MPEG frame to internal buffer or reads a frame and returns after setting a new format.
        /// </summary>
        /// <param name="num">Frame offset</param>
        /// <param name="audio">Array of decoded audio bytes</param>
        /// <param name="bytes">Number of bytes to read</param>
        /// <returns>MPG123_OK on success.</returns>
        public int DecodeFrame(ref int num, ref byte[]? audio, ref int bytes)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_DECODE"), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Get the frame
                IntPtr numPtr, bytesPtr, audioPtr = IntPtr.Zero;
                numPtr = new IntPtr(num);
                bytesPtr = new IntPtr(bytes);
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_decode_frame>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_decode_frame));
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
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DECODEFAILED"), (mpg123_errors)decodeStatus);

                return decodeStatus;
            }
        }

        /// <summary>
        /// Gets all decoders or the supported decoders
        /// </summary>
        /// <param name="onlySupported">Show only supported decoders</param>
        /// <returns>Either an array of all decoders or an array of only the supported decoders according to the current device and driver.</returns>
        public string[] GetDecoders(bool onlySupported)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var @delegate = MpgNative.GetDelegate<NativeDecoder.mpg123_supported_decoders>(MpgNative.libManagerMpg, nameof(NativeDecoder.mpg123_supported_decoders));
                var delegate2 = MpgNative.GetDelegate<NativeDecoder.mpg123_decoders>(MpgNative.libManagerMpg, nameof(NativeDecoder.mpg123_decoders));
                IntPtr decodersPtr = onlySupported ? @delegate.Invoke() : delegate2.Invoke();
                string[] decoders = ArrayVariantLength.GetStringsUnknownLength(decodersPtr);
                return decoders;
            }
        }

        /// <summary>
        /// Gets the current decoder
        /// </summary>
        /// <returns></returns>
        public string GetCurrentDecoder()
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeDecoder.mpg123_current_decoder>(MpgNative.libManagerMpg, nameof(NativeDecoder.mpg123_current_decoder));
                IntPtr decoderPtr = @delegate.Invoke(handle);
                return Marshal.PtrToStringAnsi(decoderPtr);
            }
        }

        /// <summary>
        /// Sets the current decoder
        /// </summary>
        /// <param name="decoderName">Decoder name</param>
        /// <exception cref="BasoliaException"></exception>
        public void SetCurrentDecoder(string decoderName)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                string[] decoders = GetDecoders(false);
                if (!decoders.Contains(decoderName))
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DECODERNOTFOUND").FormatString(decoderName), mpg123_errors.MPG123_BAD_DECODER);
                string[] supportedDecoders = GetDecoders(true);
                if (!supportedDecoders.Contains(decoderName))
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DECODERUNSUPPORTED").FormatString(decoderName), mpg123_errors.MPG123_BAD_DECODER);
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeDecoder.mpg123_decoder>(MpgNative.libManagerMpg, nameof(NativeDecoder.mpg123_decoder));
                int status = @delegate.Invoke(handle, decoderName);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_DECODERSETFAILED").FormatString(decoderName), (mpg123_errors)status);
            }
        }
        #endregion

        #region Format tools
        /// <summary>
        /// Gets the format information
        /// </summary>
        public (long rate, int channels, int encoding) GetFormatInfo()
        {
            long fileRate;
            int fileChannel, fileEncoding;
            InitBasolia.CheckInited();

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Get the rate, the number of channels, and encoding
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_getformat>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_getformat));
                int length = @delegate.Invoke(handle, out fileRate, out fileChannel, out fileEncoding);
                if (length != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILEFORMATFAILED"), mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return (fileRate, fileChannel, fileEncoding);
        }

        /// <summary>
        /// Gets the rate list supported by the library
        /// </summary>
        public int[] GetRates()
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
        public int[] GetEncodings()
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
        public string GetEncodingName(int encoding)
        {
            InitBasolia.CheckInited();
            string encodingName = "";

            // Check the encoding
            int[] encodings = GetEncodings();
            if (!encodings.Contains(encoding))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_ENCODINGNOTFOUND").FormatString(encoding), mpg123_errors.MPG123_BAD_TYPES);

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
        public string GetEncodingDescription(int encoding)
        {
            InitBasolia.CheckInited();
            string encodingDescription = "";

            // Check the encoding
            int[] encodings = GetEncodings();
            if (!encodings.Contains(encoding))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_ENCODINGNOTFOUND").FormatString(encoding), mpg123_errors.MPG123_BAD_TYPES);

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
        public FormatInfo[] GetFormats()
        {
            InitBasolia.CheckInited();
            var formats = new List<FormatInfo>();

            // We're now entering the dangerous zone
            int getStatus;
            nint fmtlist = IntPtr.Zero;
            unsafe
            {
                var outHandle = _out123Handle;

                // Get the list of supported formats
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_formats>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_formats));
                getStatus = @delegate.Invoke(outHandle, IntPtr.Zero, 0, 0, 0, ref fmtlist);
                if (getStatus == (int)out123_error.OUT123_ERR)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FORMATINFOGETFAILED"), (out123_error)getStatus);
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
        /// <param name="rate">Rate</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="channelCount">Mono, stereo, or both?</param>
        public bool IsFormatSupported(long rate, int encoding, out ChannelCount channelCount)
        {
            InitBasolia.CheckInited();

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

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
        public int GetEncodingSize(int encoding)
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
        public void NoFormat()
        {
            InitBasolia.CheckInited();

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Check for support
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_format_none>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format_none));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_OUTPUTENCODINGGETFAILED"), (mpg123_errors)resetStatus);
            }
        }

        /// <summary>
        /// Makes the underlying media handler accept all formats
        /// </summary>
        public void AllFormats()
        {
            InitBasolia.CheckInited();

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Check for support
                var @delegate = MpgNative.GetDelegate<NativeOutput.mpg123_format_all>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format_all));
                int resetStatus = @delegate.Invoke(handle);
                if (resetStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_OUTPUTFORMATSETFAILED"), (mpg123_errors)resetStatus);
            }
        }

        /// <summary>
        /// Makes the underlying media handler use this specific format
        /// </summary>
        /// <param name="rate">Rate</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="channels">Mono, stereo, or both?</param>
        public void UseFormat(long rate, ChannelCount channels, int encoding)
        {
            InitBasolia.CheckInited();

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Check for support
                var delegate2 = MpgNative.GetDelegate<NativeOutput.mpg123_format>(MpgNative.libManagerMpg, nameof(NativeOutput.mpg123_format));
                int formatStatus = delegate2.Invoke(handle, rate, (int)channels, encoding);
                if (formatStatus != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_OUTPUTENCODINGSETFAILED") + $" {rate}, {channels}, {encoding}", (mpg123_errors)formatStatus);
            }
        }
        #endregion
    }
}
