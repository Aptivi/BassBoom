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

using System;

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// The managed world version of the ID3v1 metadata class instance
    /// </summary>
    public class Id3V1Metadata
    {
        private readonly string tag = "";
        private readonly string title = "";
        private readonly string artist = "";
        private readonly string album = "";
        private readonly string year = "";
        private readonly string comment = "";
        private readonly int genreIndex;

        /// <summary>
        /// The tag
        /// </summary>
        public string Tag =>
            tag.Trim().Trim('\0');
        /// <summary>
        /// Title of the song (usually the song name)
        /// </summary>
        public string Title =>
            title.Trim().Trim('\0');
        /// <summary>
        /// Artist of the song
        /// </summary>
        public string Artist =>
            artist.Trim().Trim('\0');
        /// <summary>
        /// Album of the song
        /// </summary>
        public string Album =>
            album.Trim().Trim('\0');
        /// <summary>
        /// Release year of the song
        /// </summary>
        public string Year =>
            year.Trim().Trim('\0');
        /// <summary>
        /// A single comment for the song
        /// </summary>
        public string Comment =>
            comment.Trim().Trim('\0');
        /// <summary>
        /// Music genre
        /// </summary>
        public Id3V1Genre Genre =>
            (Id3V1Genre)(Enum.IsDefined(typeof(Id3V1Genre), GenreIndex) ? Enum.Parse(typeof(Id3V1Genre), $"{GenreIndex}") : Id3V1Genre.Unknown);
        /// <summary>
        /// Genre index
        /// </summary>
        public int GenreIndex =>
            genreIndex;

        internal Id3V1Metadata()
        { }

        internal Id3V1Metadata(string tag, string title, string artist, string album, string year, string comment, int genreIndex)
        {
            this.tag = tag;
            this.title = title;
            this.artist = artist;
            this.album = album;
            this.year = year;
            this.comment = comment;
            this.genreIndex = genreIndex;
        }
    }
}
