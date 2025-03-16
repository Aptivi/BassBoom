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

namespace BassBoom.Basolia.Exceptions
{
    /// <summary>
    /// Basolia miscellaneous exception (ones that don't have to do with libmpv or OUT123)
    /// </summary>
    public class BasoliaMiscException : Exception
    {
        /// <summary>
        /// Creates a new instance of Basolia error.
        /// </summary>
        internal BasoliaMiscException() :
            base("General Basolia error")
        { }

        /// <summary>
        /// Creates a new instance of Basolia error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        internal BasoliaMiscException(string message) :
            base(message)
        { }

        /// <summary>
        /// Creates a new instance of Basolia error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="innerException">Inner exception</param>
        internal BasoliaMiscException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }
}
