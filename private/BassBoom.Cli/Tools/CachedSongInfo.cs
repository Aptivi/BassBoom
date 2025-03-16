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

using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using System;

namespace BassBoom.Cli.Tools
{
    /// <summary>
    /// Cached song info
    /// </summary>
    internal class CachedSongInfo
    {
        /// <summary>
        /// A full path to the music file
        /// </summary>
        public string MusicPath { get; private set; }
        /// <summary>
        /// Radio station name
        /// </summary>
        public string StationName { get; private set; }
        /// <summary>
        /// Music duration in seconds
        /// </summary>
        public long Duration { get; private set; }
        /// <summary>
        /// Music duration in a string representation of the time span
        /// </summary>
        public string DurationSpan =>
            TimeSpan.FromMilliseconds(Duration).ToString();
        /// <summary>
        /// An instance of the music lyrics (if any)
        /// </summary>
        public Lyric? LyricInstance { get; private set; }
        /// <summary>
        /// Checks to see if this cached song info instance is a radio station or not
        /// </summary>
        public bool IsRadio { get; private set; }
        /// <summary>
        /// Music metadata
        /// </summary>
        public MusicMetadata? Metadata { get; private set; }
        /// <summary>
        /// Repeat checkpoint (not for radio stations)
        /// </summary>
        public TimeSpan RepeatCheckpoint { get; internal set; } = new();

        /// <summary>
        /// A cached song information
        /// </summary>
        /// <param name="musicPath">A full path to the music file</param>
        /// <param name="duration">Music duration in milliseconds</param>
        /// <param name="lyricInstance">An instance of the music lyrics (if any)</param>
        /// <param name="stationName">Radio station name</param>
        /// <param name="isRadioStation">Is this cached song info instance is a radio station or not?</param>
        /// <param name="metadata">Song metadata (null on radio stations and non-file streams)</param>
        public CachedSongInfo(string musicPath, long duration, Lyric? lyricInstance, string stationName, bool isRadioStation, MusicMetadata? metadata)
        {
            MusicPath = musicPath;
            Duration = duration;
            LyricInstance = lyricInstance;
            StationName = stationName;
            IsRadio = isRadioStation;
            Metadata = metadata;
        }
    }
}
