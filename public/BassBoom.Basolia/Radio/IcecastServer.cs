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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using BassBoom.Basolia.Exceptions;

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// An Icecast server
    /// </summary>
    [DebuggerDisplay("{ServerHostFull,nq}: S[T:{TotalStreams}|A:{ActiveStreams}] | L[{CurrentListeners}]")]
    public class IcecastServer : IRadioServer
    {
        private int totalStreams;
        private int activeStreams;
        private int currentListeners;
        private int peakListeners;
        private readonly List<StreamInfo> streams = [];
        internal JToken? streamToken;
        internal HtmlDocument streamHtmlToken = new();

        /// <inheritdoc/>
        public string ServerHost { get; }

        /// <inheritdoc/>
        public int ServerPort { get; }

        /// <inheritdoc/>
        public bool ServerHttps { get; }

        /// <inheritdoc/>
        public string ServerHostFull =>
            ServerHost + ":" + ServerPort;

        /// <inheritdoc/>
        public int TotalStreams =>
            totalStreams;

        /// <inheritdoc/>
        public int ActiveStreams =>
            activeStreams;

        /// <inheritdoc/>
        public int CurrentListeners =>
            currentListeners;

        /// <inheritdoc/>
        public int PeakListeners =>
            peakListeners;

        /// <inheritdoc/>
        public StreamInfo[] Streams =>
            [.. streams];

        /// <summary>
        /// Always <see cref="RadioServerType.Icecast"/> for Icecast radio servers
        /// </summary>
        public RadioServerType ServerType =>
            RadioServerType.Icecast;

        /// <summary>
        /// Connects to the Icecast server and gets the information
        /// </summary>
        /// <param name="serverHost">Server host name</param>
        /// <param name="serverPort">Server port</param>
        /// <param name="useHttps">Whether to use the HTTPS protocol or not</param>
        internal IcecastServer(string serverHost, int serverPort, bool useHttps)
        {
            // Check to see if we're dealing with the secure Icecast server
            ServerHost = serverHost;
            if (useHttps)
            {
                if (!serverHost.Contains(Uri.SchemeDelimiter))
                    ServerHost = $"https://{serverHost}";
            }
            else
            {
                if (!serverHost.Contains(Uri.SchemeDelimiter))
                    ServerHost = $"http://{serverHost}";
            }
            ServerHttps = useHttps;

            // Install the values initially
            ServerPort = serverPort;
        }

        /// <summary>
        /// Refreshes the statistics
        /// </summary>
        public void Refresh()
        {
            try
            {
                InitializeStatsAsync().Wait();
                FinalizeIcecast();
            }
            catch (Exception ex)
            {
                throw new BasoliaMiscException($"Failed to parse radio server {ServerHost}. More information can be found in the inner exception.", ex);
            }
        }

        /// <summary>
        /// Refreshes the statistics
        /// </summary>
        public async Task RefreshAsync()
        {
            try
            {
                await InitializeStatsAsync().ConfigureAwait(false);
                FinalizeIcecast();
            }
            catch (Exception ex)
            {
                throw new BasoliaMiscException($"Failed to parse radio server {ServerHost}. More information can be found in the inner exception.", ex);
            }
        }

        internal async Task InitializeStatsAsync()
        {
            // Use the full address to download the statistics.
            Uri statisticsUri = new(ServerHostFull + "/status-json.xsl");
            string serverResponse = await RadioTools.client.GetStringAsync(statisticsUri).ConfigureAwait(false);
            streamToken = JToken.Parse(serverResponse)["icestats"];
        }

        internal void FinalizeIcecast()
        {
            // Use all the keys in the first object except the "streams" and "version", where we'd later use the former in StreamInfo to install
            // all the streams into the new class instance.
            if (streamToken is null)
                throw new BasoliaMiscException("Stream token is null.");
            var sources = (JArray?)streamToken["source"] ??
                throw new BasoliaMiscException("There are no sources.");
            var inactive = sources.Where((token) => token["server_name"] is null);
            var active = sources.Except(inactive);
            totalStreams = sources.Count;
            activeStreams = active.Count();
            currentListeners = active.Max((token) => (int?)token["listeners"] ?? 0);
            peakListeners = active.Max((token) => (int?)token["listener_peak"] ?? 0);

            // Now, deal with the stream settings.
            streams.Clear();
            foreach (JToken stream in active)
            {
                StreamInfo streamInfo = new(this, stream);
                streams.Add(streamInfo);
            }
        }
    }
}
