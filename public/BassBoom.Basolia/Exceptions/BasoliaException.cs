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

using BassBoom.Native;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Native.Interop.Init;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Exceptions
{
    /// <summary>
    /// Basolia exception
    /// </summary>
    public class BasoliaException : Exception
    {
        /// <summary>
        /// Creates a new instance of Basolia error with the specific libmpv error.
        /// </summary>
        /// <param name="error">A libmpv error value to use.</param>
        internal BasoliaException(MpvError error) :
            base($"General Basolia error\n" +
                 $"libmpv returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeInitializer.GetDelegate<NativeError.mpv_error_string>(NativeInitializer.libManagerMpv, nameof(NativeError.mpv_error_string)).Invoke((int)error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia error with the specific libmpv error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="error">A libmpv error value to use.</param>
        internal BasoliaException(string message, MpvError error) :
            base($"{message}\n" +
                 $"libmpv returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeInitializer.GetDelegate<NativeError.mpv_error_string>(NativeInitializer.libManagerMpv, nameof(NativeError.mpv_error_string)).Invoke((int)error))}]")
        { }

        /// <summary>
        /// Creates a new instance of Basolia error with the specific libmpv error.
        /// </summary>
        /// <param name="message">Custom message to use while creating this exception</param>
        /// <param name="innerException">Inner exception</param>
        /// <param name="error">A libmpv error value to use.</param>
        internal BasoliaException(string message, Exception innerException, MpvError error) :
            base($"{message}\n" +
                 $"libmpv returned the following error: [{error} - {Marshal.PtrToStringAnsi(NativeInitializer.GetDelegate<NativeError.mpv_error_string>(NativeInitializer.libManagerMpv, nameof(NativeError.mpv_error_string)).Invoke((int)error))}]", innerException)
        { }
    }
}
