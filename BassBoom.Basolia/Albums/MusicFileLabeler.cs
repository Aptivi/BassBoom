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

using BassBoom.Basolia.File;
using System.Collections.Generic;
using System.IO;

namespace BassBoom.Basolia.Albums
{
    /// <summary>
    /// Music file labeling tools for albums
    /// </summary>
    public static class MusicFileLabeler
    {
        /// <summary>
        /// Prepends the filenames with the file number to prepare it for album folder
        /// </summary>
        /// <param name="libraryPath">The target path</param>
        public static void LabelFiles(string libraryPath)
        {
            string[] files = Directory.GetFiles(libraryPath);
            List<string> enumeratedFiles = [];

            // Filter the files that we don't need to label
            var extensions = FileTools.SupportedExtensions;
            foreach (string file in files)
            {
                if (extensions.Length > 0)
                {
                    // We have extensions to be included. Search for them.
                    string fileExtension = Path.GetExtension(file);
                    foreach (string extension in extensions)
                    {
                        // If the file has an extension that we're looking for, add it to the final list.
                        if (fileExtension == extension)
                            enumeratedFiles.Add(file);
                    }
                }
                else
                    enumeratedFiles.AddRange(files);
            }

            // Actually label the files
            for (int fileIndex = 0; fileIndex <= enumeratedFiles.Count - 1; fileIndex++)
            {
                string file = enumeratedFiles[fileIndex];
                string fileName = Path.GetFileName(file);

                // Append the current count into the file name, but we need to check how many leading zeroes we have to put so it looks like
                // the track number you usually find on music disks, like:
                //
                // "001. Artist - Song" if the last track number has three digits, or
                // "01. Artist - Song"  if the track number has two digits.
                int digits = enumeratedFiles.Count.ToString().Length;
                int fileNumber = fileIndex + 1;
                string labeledName = fileNumber.ToString().PadLeft(digits, '0') + ". " + fileName;

                // Finally, form the full path and rename it
                string labeledFilePath = Path.Combine(libraryPath, labeledName);
                if (!System.IO.File.Exists(labeledFilePath))
                {
                    System.IO.File.Move(file, labeledFilePath);
                }
            }
        }
    }
}
