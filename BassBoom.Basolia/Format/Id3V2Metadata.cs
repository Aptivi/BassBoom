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

namespace BassBoom.Basolia.Format
{
    /// <summary>
    /// The managed world version of the ID3v2 metadata class instance
    /// </summary>
    public class Id3V2Metadata
    {
        private readonly string title = "";
        private readonly string artist = "";
        private readonly string album = "";
        private readonly string year = "";
        private readonly string comment = "";
        private readonly string genre = "";
        private readonly (string, string)[] comments;
        private readonly (string, string)[] texts;
        private readonly (string, string)[] extras;
        private readonly (string, string)[] pictures;

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
        public string Genre =>
            genre.Trim().Trim('\0');
        /// <summary>
        /// List of comments
        /// </summary>
        public (string, string)[] Comments =>
            comments;
        /// <summary>
        /// List of extra text
        /// </summary>
        public (string, string)[] Texts =>
            texts;
        /// <summary>
        /// List of extras
        /// </summary>
        public (string, string)[] Extras =>
            extras;
        /// <summary>
        /// List of pictures
        /// </summary>
        public (string, string)[] Pictures =>
            pictures;

        internal Id3V2Metadata()
        { }

        internal Id3V2Metadata(string title, string artist, string album, string year, string comment, string genre, (string, string)[] comments, (string, string)[] texts, (string, string)[] extras, (string, string)[] pictures)
        {
            this.title = title;
            this.artist = artist;
            this.album = album;
            this.year = year;
            this.comment = comment;
            this.genre = genre;
            this.comments = comments;
            this.texts = texts;
            this.extras = extras;
            this.pictures = pictures;
        }
    }
}
