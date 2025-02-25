﻿//
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

namespace BassBoom.Native.Exceptions
{
    internal class BasoliaNativeLibraryException : Exception
    {
        internal BasoliaNativeLibraryException() :
            base($"Native library error\n" +
                 $"Library path is {NativeInitializer.libmpvLibPath}")
        { }

        internal BasoliaNativeLibraryException(string message) :
            base($"{message}\n" +
                 $"Library path is {NativeInitializer.libmpvLibPath}")
        { }

        internal BasoliaNativeLibraryException(string message, Exception innerException) :
            base($"{message}\n" +
                 $"Library path is {NativeInitializer.libmpvLibPath}", innerException)
        { }
    }
}
