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

namespace BassBoom.Native.Interop.Enumerations
{
    /// <summary>
    /// MPV end of file reasons
    /// </summary>
    public enum MpvEofReason
    {
        /// <summary>
        /// Indicates that we have reached the end of stream.
        /// </summary>
        MPV_END_FILE_REASON_EOF = 0,
        /// <summary>
        /// Indicates that the playback has stopped by an external action.
        /// </summary>
        MPV_END_FILE_REASON_STOP = 2,
        /// <summary>
        /// Indicates that the playback has stopped by exiting the player.
        /// </summary>
        MPV_END_FILE_REASON_QUIT = 3,
        /// <summary>
        /// Indicates that the playback has stopped due to an error.
        /// </summary>
        MPV_END_FILE_REASON_ERROR = 4,
        /// <summary>
        /// Indicates that the playback has been redirected to a new file.
        /// </summary>
        MPV_END_FILE_REASON_REDIRECT = 5,
    }
}
