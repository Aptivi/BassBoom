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

using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Playback;
using BassBoom.Basolia.Radio;
using BassBoom.Native;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Play;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BassBoom.Basolia.File
{
    /// <summary>
    /// File tools that are essential for the music player
    /// </summary>
    public static class FileTools
    {
        private static bool isOpened = false;
        private static bool isRadioStation = false;
        private static FileType? currentFile;
        private static readonly string[] supportedExts =
        [
            ".mp3",
            ".mp2",
            ".mpa",
            ".mpg",
            ".mpga",
        ];

        /// <summary>
        /// List of supported extensions
        /// </summary>
        public static string[] SupportedExtensions =>
            supportedExts;

        /// <summary>
        /// Is the file open?
        /// </summary>
        public static bool IsOpened =>
            isOpened;

        /// <summary>
        /// Is the radio station open?
        /// </summary>
        public static bool IsRadioStation =>
            isRadioStation;

        /// <summary>
        /// Current file
        /// </summary>
        public static FileType? CurrentFile =>
            currentFile;

        /// <summary>
        /// Opens a media file
        /// </summary>
        public static void OpenFile(string path)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened)
                throw new BasoliaException("Can't open this file while the current file or a radio station is still open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException("Provide a path to a music file", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if the file exists
            if (!System.IO.File.Exists(path))
                throw new BasoliaException("Music file doesn't exist", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = MpgNative._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_open>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_open));
                int openStatus = @delegate.Invoke(handle, path);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't open file", mpg123_errors.MPG123_ERR);
                isOpened = true;
            }
            currentFile = new(false, path, null, null, "");
        }

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        public static void OpenUrl(string path) =>
            Task.Run(() => OpenUrlAsync(path)).Wait();

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        public static async Task OpenUrlAsync(string path)
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (IsOpened)
                throw new BasoliaException("Can't open this URL while the current file or a radio station is still open", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException("Provide a path to a music file or a radio station", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if the radio station exists
            if (RuntimeInformation.FrameworkDescription.Contains("Framework"))
                RadioTools.client = new();
            RadioTools.client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await RadioTools.client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            RadioTools.client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaException($"This radio station doesn't exist. Error code: {(int)reply.StatusCode} ({reply.StatusCode}).", mpg123_errors.MPG123_BAD_FILE);

            // Check to see if there are any ICY headers
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaException("This doesn't look like a radio station. Are you sure?", mpg123_errors.MPG123_BAD_FILE);
            var contentType = reply.Content.Headers.ContentType;
            if (contentType.MediaType != "audio/mpeg")
                throw new BasoliaException($"This doesn't look like an MP3 radio station. You have a(n) {contentType.MediaType} type. Are you sure?", mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the radio station
                var handle = MpgNative._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_open_feed>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_open_feed));
                int openStatus = @delegate.Invoke(handle);
                if (openStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't open radio station", mpg123_errors.MPG123_ERR);
                isOpened = true;
                isRadioStation = true;
            }
            currentFile = new(true, path, await reply.Content.ReadAsStreamAsync().ConfigureAwait(false), reply.Headers, reply.Headers.GetValues("icy-name").First());

            // If necessary, feed.
            PlaybackTools.FeedRadio();
        }

        /// <summary>
        /// Closes a currently opened media file
        /// </summary>
        public static void CloseFile()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened)
                throw new BasoliaException("Can't close a file or a radio station that's already closed", mpg123_errors.MPG123_BAD_FILE);

            // First, stop the playing song
            if (PlaybackTools.State == PlaybackState.Playing || PlaybackTools.State == PlaybackState.Paused)
                PlaybackTools.Stop();

            // We're now entering the dangerous zone
            unsafe
            {
                // Close the file
                var handle = MpgNative._mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeInput.mpg123_close>(MpgNative.libManagerMpg, nameof(NativeInput.mpg123_close));
                int closeStatus = @delegate.Invoke(handle);
                if (closeStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't close file", mpg123_errors.MPG123_ERR);
                isOpened = false;
                isRadioStation = false;
                currentFile = null;
            }
        }
    }
}
