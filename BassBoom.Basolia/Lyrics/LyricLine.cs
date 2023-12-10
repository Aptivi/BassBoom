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

using System;
using System.Collections.Generic;
using System.Linq;

namespace BassBoom.Basolia.Lyrics
{
    /// <summary>
    /// A line from the lyric with its properties
    /// </summary>
    public class LyricLine
    {
        /// <summary>
        /// Lyrical line
        /// </summary>
        public string Line { get; }
        /// <summary>
        /// Starting time of the lyric line
        /// </summary>
        public TimeSpan LineSpan { get; }
        /// <summary>
        /// Group of words from the lyric line
        /// </summary>
        public List<LyricLineWord> LineWords { get; }

        protected internal LyricLine(string line, TimeSpan lineSpan)
        {
            Line = line;
            LineSpan = lineSpan;
            LineWords = LyricReader.GetLyricWords(line);

            if (line.Contains("<") && line.Contains(">"))
                Line = string.Join(" ", LineWords.Select((llw) => llw.Word).ToArray());
        }
    }
}
