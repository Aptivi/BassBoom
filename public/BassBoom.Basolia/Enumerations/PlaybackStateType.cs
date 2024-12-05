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

namespace BassBoom.Basolia.Enumerations
{
    /// <summary>
    /// Playback state type for querying
    /// </summary>
    public enum PlaybackStateType
    {
        /// <summary>
        /// Accurate state
        /// </summary>
        Accurate = 1,
        /// <summary>
        /// Buffer fill state
        /// </summary>
        BufferFill,
        /// <summary>
        /// Frankenstein state
        /// </summary>
        Frankenstein,
        /// <summary>
        /// Fresh decoder state
        /// </summary>
        FreshDecoder,
        /// <summary>
        /// Encode delay
        /// </summary>
        EncodeDelay,
        /// <summary>
        /// Encode padding
        /// </summary>
        EncodePadding,
        /// <summary>
        /// Decode delay
        /// </summary>
        DecodeDelay,
    }
}
