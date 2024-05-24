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

using BassBoom.Native.Interop.Output;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Basolia output exception
    /// </summary>
    public class BasoliaOutException : Exception
    {
        /// <summary>
        /// Creates a new instance of Basolia output error with the specific OUT123 error.
        /// </summary>
        /// <param name="error">An OUT123 error value to use.</param>
        public BasoliaOutException(out123_error error) :
            base($"General Basolia output system error\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia output error with the specific OUT123 error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="error">An OUT123 error value to use.</param>
        public BasoliaOutException(string message, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia output error with the specific OUT123 error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="innerException">Inner exception</param>
        /// <param name="error">An OUT123 error value to use.</param>
        public BasoliaOutException(string message, Exception innerException, out123_error error) :
            base($"{message}\n" +
                 $"OUT123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeOutputLib.out123_plain_strerror((int)error))}]", innerException)
        { }
    }
}
