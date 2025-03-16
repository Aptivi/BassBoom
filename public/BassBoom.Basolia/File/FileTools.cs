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
using BassBoom.Basolia.Playback;
using BassBoom.Basolia.Radio;
using BassBoom.Native;
using BassBoom.Native.Interop.Enumerations;
using SpecProbe.Software.Platform;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BassBoom.Basolia.File
{
    /// <summary>
    /// File tools that are essential for the music player
    /// </summary>
    public static class FileTools
    {
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
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static bool IsOpened(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.isOpened;
        }

        /// <summary>
        /// Is the radio station open?
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static bool IsRadioStation(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.isRadioStation;
        }

        /// <summary>
        /// Current file
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static FileType? CurrentFile(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.currentFile;
        }

        /// <summary>
        /// Opens a media file
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="path">Path to a valid media file</param>
        public static void OpenFile(BasoliaMedia? basolia, string path)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (IsOpened(basolia))
                throw new BasoliaException("Can't open this file while the current file or a radio station is still open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException("Provide a path to a music file", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file exists
            if (!System.IO.File.Exists(path))
                throw new BasoliaException("Music file doesn't exist", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = basolia._libmpvHandle;

                // TODO: Unstub this function
                basolia.isOpened = true;
            }
            basolia.currentFile = new(false, path, null, null, "");
        }

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="path">URL Path to a valid media file</param>
        public static void OpenUrl(BasoliaMedia? basolia, string path) =>
            Task.Run(() => OpenUrlAsync(basolia, path)).Wait();

        /// <summary>
        /// Opens a remote radio station
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="path">URL Path to a valid media file</param>
        public static async Task OpenUrlAsync(BasoliaMedia? basolia, string path)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (IsOpened(basolia))
                throw new BasoliaException("Can't open this URL while the current file or a radio station is still open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we provided a path
            if (string.IsNullOrEmpty(path))
                throw new BasoliaException("Provide a path to a music file or a radio station", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the radio station exists
            if (PlatformHelper.IsDotNetFx())
                RadioTools.client = new();
            RadioTools.client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await RadioTools.client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            RadioTools.client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaException($"This radio station doesn't exist. Error code: {(int)reply.StatusCode} ({reply.StatusCode}).", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if there are any ICY headers
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaException("This doesn't look like a radio station. Are you sure?", MpvError.MPV_ERROR_INVALID_PARAMETER);
            var contentType = reply.Content.Headers.ContentType;
            if (contentType.MediaType != "audio/mpeg")
                throw new BasoliaException($"This doesn't look like an MP3 radio station. You have a(n) {contentType.MediaType} type. Are you sure?", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the radio station
                var handle = basolia._libmpvHandle;

                // TODO: Unstub this function
                basolia.isOpened = true;
                basolia.isRadioStation = true;
            }
            basolia.currentFile = new(true, path, await reply.Content.ReadAsStreamAsync().ConfigureAwait(false), reply.Headers, reply.Headers.GetValues("icy-name").First());

            // If necessary, feed.
            PlaybackTools.FeedRadio(basolia);
        }

        /// <summary>
        /// Opens a stream
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="audioStream">MPEG audio stream</param>
        public static void OpenFrom(BasoliaMedia? basolia, Stream? audioStream) =>
            Task.Run(() => OpenFromAsync(basolia, audioStream)).Wait();

        /// <summary>
        /// Opens a stream
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="audioStream">MPEG audio stream</param>
        public static async Task OpenFromAsync(BasoliaMedia? basolia, Stream? audioStream)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (IsOpened(basolia))
                throw new BasoliaException("Can't open this URL while the current file or a radio station is still open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if we provided a stream
            if (audioStream is null)
                throw new BasoliaException("Audio stream is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the stream
                var handle = basolia._libmpvHandle;

                // TODO: Unstub this function
                basolia.isOpened = true;
            }
            basolia.currentFile = new(false, "", audioStream, null, "");

            // If necessary, feed.
            await PlaybackTools.FeedStream(basolia);
        }

        /// <summary>
        /// Closes a currently opened media file
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static void CloseFile(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!IsOpened(basolia))
                throw new BasoliaException("Can't close a file or a radio station that's already closed", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // First, stop the playing song
            var state = PlaybackTools.GetState(basolia);
            if (state == PlaybackState.Playing || state == PlaybackState.Paused)
                PlaybackTools.Stop(basolia);

            // We're now entering the dangerous zone
            unsafe
            {
                // Close the file
                var handle = basolia._libmpvHandle;

                // TODO: Unstub this function
                basolia.isOpened = false;
                basolia.isRadioStation = false;
                basolia.currentFile?.Stream?.Close();
                basolia.currentFile = null;
            }
        }
    }
}
