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

using BassBoom.Native.Interop.Synthesis;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Basolia synthesis exception
    /// </summary>
    public class BasoliaSynException : Exception
    {
        /// <summary>
        /// Creates a new instance of Basolia synthesis error with the specific SYN123 error.
        /// </summary>
        /// <param name="error">An SYN123 error value to use.</param>
        public BasoliaSynException(syn123_error error) :
            base($"General Basolia synthesis system error\n" +
                 $"SYN123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeSynthesis.syn123_strerror(error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia synthesis error with the specific SYN123 error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="error">An SYN123 error value to use.</param>
        public BasoliaSynException(string message, syn123_error error) :
            base($"{message}\n" +
                 $"SYN123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeSynthesis.syn123_strerror(error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia synthesis error with the specific SYN123 error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="innerException">Inner exception</param>
        /// <param name="error">An SYN123 error value to use.</param>
        public BasoliaSynException(string message, Exception innerException, syn123_error error) :
            base($"{message}\n" +
                 $"SYN123 returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeSynthesis.syn123_strerror(error))}]", innerException)
        { }
    }
}
