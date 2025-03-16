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

namespace BassBoom.Cli.Tools
{
    /// <summary>
    /// Music metadata
    /// </summary>
    internal class MusicMetadata
    {
        /// <summary>
        /// Title of the music (output from the media-title property)
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Artist of the music (output from one of the metadata property sub-keys)
        /// </summary>
        public string Artist { get; private set; }

        /// <summary>
        /// A cached song information
        /// </summary>
        /// <param name="title">Title of the music (output from the media-title property)</param>
        /// <param name="artist">Artist of the music (output from one of the metadata property sub-keys)</param>
        public MusicMetadata(string title, string artist)
        {
            Title = title;
            Artist = artist;
        }
    }
}
