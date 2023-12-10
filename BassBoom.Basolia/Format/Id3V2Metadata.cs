//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
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
    public class Id3V2Metadata
    {
        private string title = "";
        private string artist = "";
        private string album = "";
        private string year = "";
        private string comment = "";
        private string genre = "";
        private (string, string)[] comments;
        private (string, string)[] texts;
        private (string, string)[] extras;
        private (string, string)[] pictures;

        public string Title =>
            title.Trim().Trim('\0');
        public string Artist =>
            artist.Trim().Trim('\0');
        public string Album =>
            album.Trim().Trim('\0');
        public string Year =>
            year.Trim().Trim('\0');
        public string Comment =>
            comment.Trim().Trim('\0');
        public string Genre =>
            genre.Trim().Trim('\0');
        public (string, string)[] Comments =>
            comments;
        public (string, string)[] Texts =>
            texts;
        public (string, string)[] Extras =>
            extras;
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
