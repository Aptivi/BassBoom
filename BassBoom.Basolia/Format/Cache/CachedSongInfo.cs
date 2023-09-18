
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Basolia.Lyrics;

namespace BassBoom.Basolia.Format.Cache
{
    /// <summary>
    /// Cached song info
    /// </summary>
    public class CachedSongInfo
    {
        public string MusicPath { get; private set; }
        public Id3V1Metadata MetadataV1 { get; private set; }
        public Id3V2Metadata MetadataV2 { get; private set; }
        public int Duration { get; private set; }
        public (long rate, int channels, int encoding) FormatInfo { get; private set; }
        public FrameInfo FrameInfo { get; private set; }
        public Lyric LyricInstance { get; private set; }
        public string DurationSpan =>
            AudioInfoTools.GetDurationSpanFromSamples(Duration, FormatInfo).ToString();

        public CachedSongInfo(string musicPath, Id3V1Metadata metadataV1, Id3V2Metadata metadataV2, int duration, (long rate, int channels, int encoding) formatInfo, FrameInfo frameInfo, Lyric lyricInstance)
        {
            MusicPath = musicPath;
            MetadataV1 = metadataV1;
            MetadataV2 = metadataV2;
            Duration = duration;
            FormatInfo = formatInfo;
            FrameInfo = frameInfo;
            LyricInstance = lyricInstance;
        }
    }
}
