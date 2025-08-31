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

namespace BassBoom.Basolia.Playback.Playlists.Enumerations
{
    /// <summary>
    /// Song type
    /// </summary>
    public enum SongType
    {
        /// <summary>
        /// Song found within a file (seekable)
        /// </summary>
        File,
        /// <summary>
        /// Song found within a URL (depends)
        /// </summary>
        URL,
        /// <summary>
        /// Radio station found within a streaming URL (unseekable)
        /// </summary>
        Radio,
    }
}
