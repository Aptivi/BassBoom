﻿//
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

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// Happens when the error in parsing the stream has occurred
    /// </summary>
    public class ShoutcastStreamParseException : Exception
    {
        /// <summary>
        /// Throws the stream parse exception
        /// </summary>
        public ShoutcastStreamParseException()
        { }
        /// <summary>
        /// Throws the stream parse exception
        /// </summary>
        public ShoutcastStreamParseException(string message) :
            base(message)
        { }
        /// <summary>
        /// Throws the stream parse exception
        /// </summary>
        public ShoutcastStreamParseException(string message, Exception inner) :
            base(message, inner)
        { }
    }
}
