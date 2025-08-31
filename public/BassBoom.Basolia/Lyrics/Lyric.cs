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

using BassBoom.Basolia.Playback;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BassBoom.Basolia.Lyrics
{
    /// <summary>
    /// A song lyric class containing lyrical lines
    /// </summary>
    public class Lyric
    {
        /// <summary>
        /// Lyric lines
        /// </summary>
        public List<LyricLine> Lines { get; }

        /// <summary>
        /// Gets all the lines from the start to the current music duration
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Array of lyric lines from the start to the current music duration</returns>
        public LyricLine[] GetLinesCurrent(BasoliaMedia? basolia)
        {
            var currentSpan = PlaybackPositioningTools.GetCurrentDurationSpan(basolia);
            return GetLinesToSpan(currentSpan);
        }

        /// <summary>
        /// Gets all the lines from the current music duration to the end
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Array of lyric lines from the current music duration to the end</returns>
        public LyricLine[] GetLinesUpcoming(BasoliaMedia? basolia)
        {
            var currentSpan = PlaybackPositioningTools.GetCurrentDurationSpan(basolia);
            return GetLinesFromSpan(currentSpan);
        }

        /// <summary>
        /// Gets the last lyric line from the current music duration
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Last lyric line from the current music duration</returns>
        public string GetLastLineCurrent(BasoliaMedia? basolia)
        {
            var processedLines = GetLinesCurrent(basolia);
            if (processedLines.Length > 0)
                return processedLines[processedLines.Length - 1].Line;
            return "";
        }

        /// <summary>
        /// Gets the last lyric line words from the current music duration
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Last lyric line word from the current music duration</returns>
        public List<LyricLineWord> GetLastLineWordsCurrent(BasoliaMedia? basolia)
        {
            var processedLines = GetLinesCurrent(basolia);
            if (processedLines.Length > 0)
                return processedLines[processedLines.Length - 1].LineWords;
            return [];
        }

        /// <summary>
        /// Gets all the lines from the start to the current span
        /// </summary>
        /// <param name="span">Time span in which it usually represents the current music duration</param>
        /// <returns>Array of lyric lines from the start to the current span</returns>
        public LyricLine[] GetLinesToSpan(TimeSpan span) =>
            Lines.Where((line) => line.LineSpan <= span).ToArray();

        /// <summary>
        /// Gets all the lines from the current span to the end
        /// </summary>
        /// <param name="span">Time span in which it usually represents the current music duration</param>
        /// <returns>Array of lyric lines from the current span to the end</returns>
        public LyricLine[] GetLinesFromSpan(TimeSpan span) =>
            Lines.Where((line) => line.LineSpan > span).ToArray();

        /// <summary>
        /// Gets the last lyric line from the given time span
        /// </summary>
        /// <param name="span">Time span in which it usually represents the current music duration</param>
        /// <returns>Last lyric line from the given time span</returns>
        public string GetLastLineAtSpan(TimeSpan span)
        {
            var processedLines = GetLinesToSpan(span);
            if (processedLines.Length > 0)
                return processedLines[processedLines.Length - 1].Line;
            return "";
        }

        /// <summary>
        /// Gets the last lyric line words from the given time span
        /// </summary>
        /// <param name="span">Time span in which it usually represents the current music duration</param>
        /// <returns>Last lyric line word from the given time span</returns>
        public List<LyricLineWord> GetLastLineWordsAtSpan(TimeSpan span)
        {
            var processedLines = GetLinesToSpan(span);
            if (processedLines.Length > 0)
                return processedLines[processedLines.Length - 1].LineWords;
            return [];
        }

        internal Lyric(List<LyricLine> lines)
        {
            Lines = lines;
        }
    }
}
