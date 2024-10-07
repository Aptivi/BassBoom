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

using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback;
using System.IO;

namespace BassBoom.Basolia.Independent
{
    /// <summary>
    /// Play and Forget class (to initialize playback and to forget)
    /// </summary>
    public static class PlayForget
    {
        /// <summary>
        /// Plays the file
        /// </summary>
        /// <param name="path">Path to a music file</param>
        /// <param name="settings">Settings of the play/forget technique</param>
        public static void PlayFile(string path, PlayForgetSettings? settings = null)
        {
            settings ??= new();

            // Make a Basolia media instance and open it with a file
            var media = new BasoliaMedia(settings.RootLibPath);
            FileTools.OpenFile(media, path);

            // Play and forget
            PlayAndForget(media, settings);
        }

        /// <summary>
        /// Plays the stream
        /// </summary>
        /// <param name="stream">Stream that contains valid MPEG audio stream</param>
        /// <param name="settings">Settings of the play/forget technique</param>
        public static void PlayStream(Stream stream, PlayForgetSettings? settings = null)
        {
            settings ??= new();

            // Make a Basolia media instance and open it with a file
            var media = new BasoliaMedia(settings.RootLibPath);
            FileTools.OpenFrom(media, stream);

            // Play and forget
            PlayAndForget(media, settings);
        }

        internal static void PlayAndForget(BasoliaMedia media, PlayForgetSettings settings)
        {
            // Set the volume
            PlaybackTools.SetVolume(media, settings.Volume, settings.VolumeBoost);

            // Play the file
            PlaybackTools.Play(media);

            // Close the file
            FileTools.CloseFile(media);
        }
    }
}
