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

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Languages;
using BassBoom.Basolia.Media.File;
using BassBoom.Basolia.Media.Playback;
using BassBoom.Basolia.Media.Radio;
using BassBoom.Native;
using BassBoom.Native.Interop.Init;
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
        /// <summary>
        /// List of supported extensions
        /// </summary>
        public static string[] SupportedExtensions =>
            [
                ".mp3",
                ".mp2",
                ".mpa",
                ".mpg",
                ".mpga",
            ];

        /// <summary>
        /// Is the file open?
        /// </summary>
        public bool IsOpened()
        {
            InitBasolia.CheckInited();
            return isOpened;
        }

        /// <summary>
        /// Is the radio station open?
        /// </summary>
        public bool IsRadioStation()
        {
            InitBasolia.CheckInited();
            return isRadioStation;
        }

        /// <summary>
        /// Current file
        /// </summary>
        public FileType? CurrentFile()
        {
            InitBasolia.CheckInited();
            return currentFile;
        }

        /// <summary>
        /// Opens a media file
        /// </summary>
        /// <param name="path">Path to a valid media file</param>
        public void OpenFile(string path)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_FILEALREADYOPEN"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NEEDSMUSICFILEPATH"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if the file exists
            if (!System.IO.File.Exists(path))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_MUSICFILENOTFOUND"), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_open>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_open));
                int openStatus = @delegate.Invoke(handle, path);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_MUSICFILEOPENFAILED"), mpg123_errors.MPG123_ERR);
                isOpened = true;
            }
            currentFile = new(false, path, null, null, "");
        }

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        /// <param name="path">URL Path to a valid media file</param>
        public void OpenUrl(string path) =>
            Task.Run(() => OpenUrlAsync(path)).Wait();

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        /// <param name="path">URL Path to a valid media file</param>
        public async Task OpenUrlAsync(string path)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_URLALREADYOPEN"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NEEDSMUSICURL"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if the radio station exists
            if (PlatformHelper.IsDotNetFx())
                RadioTools.client = new();
            RadioTools.client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await RadioTools.client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            RadioTools.client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NORADIOSTATION") + $" {(int)reply.StatusCode} ({reply.StatusCode}).", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if there are any ICY headers
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NOTARADIOSTATION"), mpg123_errors.MPG123_BAD_FILE);
            var contentType = reply.Content.Headers.ContentType;
            if (contentType.MediaType != "audio/mpeg")
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NOTMPEGRADIOSTATION").FormatString(contentType.MediaType), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the radio station
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_open_feed>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_open_feed));
                int openStatus = @delegate.Invoke(handle);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_MUSICRADIOOPENFAILED"), mpg123_errors.MPG123_ERR);
                isOpened = true;
                isRadioStation = true;
            }
            currentFile = new(true, path, await reply.Content.ReadAsStreamAsync().ConfigureAwait(false), reply.Headers, reply.Headers.GetValues("icy-name").First());

            // If necessary, feed.
            FeedRadio();
        }

        /// <summary>
        /// Opens a stream
        /// </summary>
        /// <param name="audioStream">MPEG audio stream</param>
        public void OpenFrom(Stream? audioStream) =>
            Task.Run(() => OpenFromAsync(audioStream)).Wait();

        /// <summary>
        /// Opens a stream
        /// </summary>
        /// <param name="audioStream">MPEG audio stream</param>
        public async Task OpenFromAsync(Stream? audioStream)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_URLALREADYOPEN"), mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a stream
            if (audioStream is null)
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_NEEDSAUDIOSTREAM"), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the stream
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_open_feed>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_open_feed));
                int openStatus = @delegate.Invoke(handle);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_MUSICSTREAMOPENFAILED"), mpg123_errors.MPG123_ERR);
                isOpened = true;
            }
            currentFile = new(false, "", audioStream, null, "");

            // If necessary, feed.
            await FeedStream();
        }

        /// <summary>
        /// Closes a currently opened media file
        /// </summary>
        public void CloseFile()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_ALREADYCLOSED"), mpg123_errors.MPG123_BAD_FILE);

            // First, stop the playing song
            var state = GetState();
            if (state == PlaybackState.Playing || state == PlaybackState.Paused)
                Stop();

            // We're now entering the dangerous zone
            unsafe
            {
                // Close the file
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_close>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_close));
                int closeStatus = @delegate.Invoke(handle);
                if (closeStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FILE_EXCEPTION_CLOSEFAILED"), mpg123_errors.MPG123_ERR);
                isOpened = false;
                isRadioStation = false;
                currentFile?.Stream?.Close();
                currentFile = null;
            }
        }
    }
}
