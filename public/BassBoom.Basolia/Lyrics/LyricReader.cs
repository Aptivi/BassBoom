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
using System.Collections.Generic;
using System.IO;
using FileIO = System.IO.File;

namespace BassBoom.Basolia.Lyrics
{
    /// <summary>
    /// Lyrics reader module
    /// </summary>
    public static class LyricReader
    {
        /// <summary>
        /// Gets the lyrics and their properties from the .LRC or .TXT lyric file
        /// </summary>
        /// <param name="path">Path to the LRC or TXT file containing lyrics</param>
        public static Lyric GetLyrics(string path)
        {
            // Check to see if the lyrics path exists
            if (!FileIO.Exists(path))
                throw new FileNotFoundException("Lyric doesn't exist", path);

            // Get the lines and parse them
            var lyricFileLines = FileIO.ReadAllLines(path);
            var lyricLines = new List<LyricLine>();
            foreach (var line in lyricFileLines)
            {
                // Lyric line is usually [00:00.00]Some lyric lines here
                string finalLine = line.Trim();

                // Check the line
                if (string.IsNullOrWhiteSpace(finalLine))
                    // Don't process empty line
                    continue;
                if (!finalLine.StartsWith("["))
                    // Don't process non-lyric info start line
                    continue;
                if (finalLine.Length == finalLine.IndexOf("]") + 1)
                    // Don't process lyric info without lyric line
                    continue;

                // We need to trim it after splitting the two elements
                string period = "00:" + finalLine.Substring(finalLine.IndexOf("[") + 1, finalLine.IndexOf("]") - 1);
                string text = finalLine.Substring(finalLine.IndexOf("]") + 1).Trim();

                // Parse the period and install the values to the LyricLine
                var periodTs = TimeSpan.Parse(period);
                var lyricLine = new LyricLine(text, periodTs);

                // Add the line
                lyricLines.Add(lyricLine);
            }

            // Return the lyric
            return new Lyric(lyricLines);
        }

        #region Internal functions
        internal static List<LyricLineWord> GetLyricWords(string line)
        {
            var lyricWords = new List<LyricLineWord>();
            var words = line.Split(' ');

            // If the line is in the <Time> format for each word, take them and install them
            for (int i = 0; i < words.Length; i++)
            {
                string timeOrWord = words[i];
                string nextWord = "";
                bool isTime = timeOrWord.Contains("<") && timeOrWord.Contains(">");
                var wordTime = new TimeSpan();

                // If the current word is a time, populate the word variable
                if (isTime)
                {
                    // Strip the time indicators and get the next word
                    timeOrWord = timeOrWord.Replace("<", "").Replace(">", "");
                    nextWord = words[i + 1];
                    i++;

                    // Parse the time
                    wordTime = TimeSpan.Parse($"00:{timeOrWord}");
                }

                // Get the final word and install the values and then add to the dictionary
                string finalWord = isTime ? nextWord : timeOrWord;
                var lyricLineWord = new LyricLineWord(finalWord, wordTime);
                lyricWords.Add(lyricLineWord);
            }

            // Return the final list
            return lyricWords;
        }
        #endregion
    }
}
