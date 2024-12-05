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

using System.Threading.Tasks;

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// Radio server interface
    /// </summary>
    public interface IRadioServer
    {
        /// <summary>
        /// Server IP address
        /// </summary>
        public string ServerHost { get; }

        /// <summary>
        /// Server port
        /// </summary>
        public int ServerPort { get; }

        /// <summary>
        /// Whether the Shoutcast server is using HTTPS or not
        /// </summary>
        public bool ServerHttps { get; }

        /// <summary>
        /// Server IP address with port
        /// </summary>
        public string ServerHostFull { get; }

        /// <summary>
        /// Server type (SHOUTcast, IceCast, ...)
        /// </summary>
        public RadioServerType ServerType { get; }

        /// <summary>
        /// Total number of streams in the server
        /// </summary>
        public int TotalStreams { get; }

        /// <summary>
        /// Active streams in the server
        /// </summary>
        public int ActiveStreams { get; }

        /// <summary>
        /// How many people are listening to the server at this time?
        /// </summary>
        public int CurrentListeners { get; }

        /// <summary>
        /// How many listeners did the server ever get at peak times?
        /// </summary>
        public int PeakListeners { get; }

        /// <summary>
        /// Available streams and their statistics
        /// </summary>
        public StreamInfo[] Streams { get; }

        /// <summary>
        /// Refreshes the statistics
        /// </summary>
        public void Refresh();

        /// <summary>
        /// Refreshes the statistics
        /// </summary>
        public Task RefreshAsync();
    }
}
