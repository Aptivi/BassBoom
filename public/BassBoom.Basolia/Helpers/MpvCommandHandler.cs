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

using BassBoom.Basolia.Exceptions;
using BassBoom.Native;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Native.Interop.Init;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Helpers
{
    /// <summary>
    /// MPV command handler
    /// </summary>
    public static class MpvCommandHandler
    {
        /// <summary>
        /// Runs an MPV command
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="commandArgs">Command and its arguments to pass to the library</param>
        /// <exception cref="BasoliaException"></exception>
        public static void RunCommand(BasoliaMedia? basolia, params string[] commandArgs)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Open the file
                var handle = basolia._libmpvHandle;
                var argPointer = NativeArrayBuilder.InitByteArrayPointers(commandArgs, out var argPointers);
                MpvError commandResult = (MpvError)NativeInitializer.GetDelegate<NativeInit.mpv_command>(NativeInitializer.libManagerMpv, nameof(NativeInit.mpv_command)).Invoke(handle, argPointer);
                foreach (var ptr in argPointers)
                    Marshal.FreeHGlobal(ptr);
                Marshal.FreeHGlobal(argPointer);
                if (commandResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to execute command [{string.Join(", ", commandArgs)}]", commandResult);
            }
        }
    }
}
