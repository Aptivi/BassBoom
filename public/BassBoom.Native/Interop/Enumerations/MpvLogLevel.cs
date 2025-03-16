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
    /// MPV log level
    /// </summary>
    public enum MpvLogLevel
    {
        /// <summary>
        /// No logging
        /// </summary>
        MPV_LOG_LEVEL_NONE = 0,
        /// <summary>
        /// Fatal errors
        /// </summary>
        MPV_LOG_LEVEL_FATAL = 10,
        /// <summary>
        /// Continuable errors
        /// </summary>
        MPV_LOG_LEVEL_ERROR = 20,
        /// <summary>
        /// Warning messages
        /// </summary>
        MPV_LOG_LEVEL_WARN = 30,
        /// <summary>
        /// Informational messages
        /// </summary>
        MPV_LOG_LEVEL_INFO = 40,
        /// <summary>
        /// Verbose messages
        /// </summary>
        MPV_LOG_LEVEL_V = 50,
        /// <summary>
        /// Debug messages
        /// </summary>
        MPV_LOG_LEVEL_DEBUG = 60,
        /// <summary>
        /// Deeper debug messages
        /// </summary>
        MPV_LOG_LEVEL_TRACE = 70,
    }
}
