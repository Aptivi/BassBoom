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

using BassBoom.Native.Interop.Init;

namespace BassBoom.Native.Interop.Enumerations
{
    /// <summary>
    /// MPV error codes
    /// </summary>
    public enum MpvError
    {
        /// <summary>
        /// Indicates that the operation is successful. This can be either zero or greater than zero.
        /// </summary>
        MPV_ERROR_SUCCESS = 0,
        /// <summary>
        /// Indicates that the event ringbuffer is full and that Basolia can't process any more events.
        /// </summary>
        MPV_ERROR_EVENT_QUEUE_FULL = -1,
        /// <summary>
        /// Indicates that the memory allocation has failed.
        /// </summary>
        MPV_ERROR_NOMEM = -2,
        /// <summary>
        /// Indicates that the MPV core wasn't initialized yet. Please call <see cref="NativeInit.mpv_create"/>
        /// </summary>
        MPV_ERROR_UNINITIALIZED = -3,
        /// <summary>
        /// Indicates that the parameter is invalid or the operation failed for some reason.
        /// </summary>
        MPV_ERROR_INVALID_PARAMETER = -4,
        /// <summary>
        /// Indicates that the option is invalid.
        /// </summary>
        MPV_ERROR_OPTION_NOT_FOUND = -5,
        /// <summary>
        /// Indicates that the client is trying to set an option using the unsupported MPV format.
        /// </summary>
        MPV_ERROR_OPTION_FORMAT = -6,
        /// <summary>
        /// Indicates that the client is trying to set an option using the invalid option value.
        /// </summary>
        MPV_ERROR_OPTION_ERROR = -7,
        /// <summary>
        /// Indicates that the property is invalid.
        /// </summary>
        MPV_ERROR_PROPERTY_NOT_FOUND = -8,
        /// <summary>
        /// Indicates that the client is trying to set a property using the unsupported MPV format.
        /// </summary>
        MPV_ERROR_PROPERTY_FORMAT = -9,
        /// <summary>
        /// Indicates that the property is unavailable at this time.
        /// </summary>
        MPV_ERROR_PROPERTY_UNAVAILABLE = -10,
        /// <summary>
        /// Indicates that the property can't be get or set.
        /// </summary>
        MPV_ERROR_PROPERTY_ERROR = -11,
        /// <summary>
        /// Indicates that the command can't be run due to some error.
        /// </summary>
        MPV_ERROR_COMMAND = -12,
        /// <summary>
        /// Indicates that loading has failed (usually used with <c>mpv_event_end_file.error</c>).
        /// </summary>
        MPV_ERROR_LOADING_FAILED = -13,
        /// <summary>
        /// Indicates that the audio output has failed.
        /// </summary>
        MPV_ERROR_AO_INIT_FAILED = -14,
        /// <summary>
        /// Indicates that the video output has failed.
        /// </summary>
        MPV_ERROR_VO_INIT_FAILED = -15,
        /// <summary>
        /// Indicates that there are no more streams to play.
        /// </summary>
        MPV_ERROR_NOTHING_TO_PLAY = -16,
        /// <summary>
        /// Indicates that the audio/video format is unrecognized (probably due to corrupt file).
        /// </summary>
        MPV_ERROR_UNKNOWN_FORMAT = -17,
        /// <summary>
        /// Indicates that the system requirements for performing this operation have not been met.
        /// </summary>
        MPV_ERROR_UNSUPPORTED = -18,
        /// <summary>
        /// Indicates that the native API function wasn't implemented yet.
        /// </summary>
        MPV_ERROR_NOT_IMPLEMENTED = -19,
        /// <summary>
        /// Indicates that the MPV library has raised some unknown error.
        /// </summary>
        MPV_ERROR_GENERIC = -20
    }
}
