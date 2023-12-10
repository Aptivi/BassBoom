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

using BassBoom.Native.Interop.Output;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia
{
    public class BasoliaOutException : Exception
    {
        public BasoliaOutException(out123_error error) :
            base($"General Basolia output system error\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]")
        { }

        public BasoliaOutException(string message, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]")
        { }

        public BasoliaOutException(string message, Exception innerException, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]", innerException)
        { }
    }
}
