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
using System.IO;
using System.Net.Http.Headers;

namespace BassBoom.Basolia.File
{
    /// <summary>
    /// File type class
    /// </summary>
    public class FileType
    {
        private bool isLink;
        private string path;
        private Stream? stream;
        private HttpResponseHeaders? headers;
        private string stationName;

        /// <summary>
        /// Is this file type a link?
        /// </summary>
        public bool IsLink =>
            isLink;

        /// <summary>
        /// Path to either a local MP3 file or a radio station
        /// </summary>
        public string Path =>
            path;

        /// <summary>
        /// Radio station stream
        /// </summary>
        public Stream? Stream =>
            stream;

        /// <summary>
        /// Radio station ICY headers
        /// </summary>
        public HttpResponseHeaders? Headers =>
            headers;

        /// <summary>
        /// Radio station name
        /// </summary>
        public string StationName =>
            stationName;

        internal FileType(bool isLink, string path, Stream? stream, HttpResponseHeaders? headers, string stationName)
        {
            this.isLink = isLink;
            this.path = path ?? throw new ArgumentNullException(nameof(path));
            this.stream = stream;
            this.headers = headers;
            this.stationName = stationName;
        }
    }
}
