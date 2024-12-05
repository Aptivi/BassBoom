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

using BassBoom.Basolia.Playback.Playlists.Enumerations;

namespace BassBoom.Basolia.Playback.Playlists.Instances
{
    /// <summary>
    /// Track information
    /// </summary>
    public class TrackInfo
    {
        private readonly int trackSeconds = 0;
        private readonly string trackName = "";
        private readonly string trackPath = "";
        private readonly SongType trackType = SongType.File;

        /// <summary>
        /// Duration of this track in seconds. Always -1 for Internet streams.
        /// </summary>
        public int Duration =>
            trackSeconds;

        /// <summary>
        /// Name of this track
        /// </summary>
        public string Name =>
            trackName;

        /// <summary>
        /// Path of this track
        /// </summary>
        public string Path =>
            trackPath;

        /// <summary>
        /// Type of this track
        /// </summary>
        public SongType Type =>
            trackType;

        internal TrackInfo(int trackSeconds, string trackName, string trackPath, SongType trackType)
        {
            this.trackSeconds = trackSeconds;
            this.trackName = trackName;
            this.trackPath = trackPath;
            this.trackType = trackType;
        }
    }
}
