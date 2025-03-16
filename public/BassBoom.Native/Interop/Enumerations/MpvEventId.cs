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
    /// MPV event ID list
    /// </summary>
    public enum MpvEventId
    {
        /// <summary>
        /// Empty event
        /// </summary>
        MPV_EVENT_NONE = 0,
        /// <summary>
        /// The player is shutting down and user code should clean up and use <see cref="NativeInit.mpv_destroy"/> or <see cref="NativeInit.mpv_terminate_destroy"/> delegate wrappers found in Basolia.
        /// </summary>
        MPV_EVENT_SHUTDOWN = 1,
        /// <summary>
        /// Log message event
        /// </summary>
        MPV_EVENT_LOG_MESSAGE = 2,
        /// <summary>
        /// Response to calls to property get requests (async)
        /// </summary>
        MPV_EVENT_GET_PROPERTY_REPLY = 3,
        /// <summary>
        /// Response to calls to property set requests (async)
        /// </summary>
        MPV_EVENT_SET_PROPERTY_REPLY = 4,
        /// <summary>
        /// Response to calls to command requests (async)
        /// </summary>
        MPV_EVENT_COMMAND_REPLY = 5,
        /// <summary>
        /// Start of a file before playback event
        /// </summary>
        MPV_EVENT_START_FILE = 6,
        /// <summary>
        /// End of a file after playback event
        /// </summary>
        MPV_EVENT_END_FILE = 7,
        /// <summary>
        /// File loaded and decoding starts event
        /// </summary>
        MPV_EVENT_FILE_LOADED = 8,
        /// <summary>
        /// [Generally not needed] Idle event
        /// </summary>
        MPV_EVENT_IDLE = 11,
        /// <summary>
        /// [Generally not needed] Tick event
        /// </summary>
        MPV_EVENT_TICK = 14,
        /// <summary>
        /// script-message input command event
        /// </summary>
        MPV_EVENT_CLIENT_MESSAGE = 16,
        /// <summary>
        /// Video reconfiguration event
        /// </summary>
        MPV_EVENT_VIDEO_RECONFIG = 17,
        /// <summary>
        /// Audio reconfiguration event
        /// </summary>
        MPV_EVENT_AUDIO_RECONFIG = 18,
        /// <summary>
        /// Playback seek (temporary stop) event
        /// </summary>
        MPV_EVENT_SEEK = 20,
        /// <summary>
        /// Playback seek finished (playback restart) event
        /// </summary>
        MPV_EVENT_PLAYBACK_RESTART = 21,
        /// <summary>
        /// Observed property changed event
        /// </summary>
        MPV_EVENT_PROPERTY_CHANGE = 22,
        /// <summary>
        /// Event queue overflow event
        /// </summary>
        MPV_EVENT_QUEUE_OVERFLOW = 24,
        /// <summary>
        /// Event hook response event
        /// </summary>
        MPV_EVENT_HOOK = 25,
    }
}
