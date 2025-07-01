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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Diagnostics;
using BassBoom.Basolia.Exceptions;
using Textify.General;

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// A Shoutcast server
    /// </summary>
    [DebuggerDisplay("{ServerHostFull,nq}: S[T:{TotalStreams}|A:{ActiveStreams}] | L[{CurrentListeners}]")]
    public class ShoutcastServer : IRadioServer
    {
        private ShoutcastVersion serverVersion;
        private int totalStreams;
        private int activeStreams;
        private int currentListeners;
        private int peakListeners;
        private int maxListeners;
        private int uniqueListeners;
        private int averageTime;
        private string mimeType = "";
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

        /// <inheritdoc/>
        public string MimeType =>
            mimeType;

        /// <summary>
        /// How many people can listen to the server?
        /// </summary>
        public int MaxListeners =>
            maxListeners;

        /// <summary>
        /// How many unique listeners are there?
        /// </summary>
        public int UniqueListeners =>
            uniqueListeners;

        /// <summary>
        /// Average time on any active listener connections in seconds
        /// </summary>
        public int AverageTime =>
            averageTime;

        /// <summary>
        /// Average time on any active listener connections in the time span
        /// </summary>
        public TimeSpan AverageTimeSpan =>
            TimeSpan.FromSeconds(AverageTime);

        /// <summary>
        /// Always <see cref="RadioServerType.Shoutcast"/> for SHOUTcast radio servers
        /// </summary>
        public RadioServerType ServerType =>
            RadioServerType.Shoutcast;

        /// <summary>
        /// Server version (1.x, 2.x)
        /// </summary>
        public ShoutcastVersion ServerVersion =>
            serverVersion;

        /// <summary>
        /// Connects to the Shoutcast server and gets the information
        /// </summary>
        /// <param name="serverHost">Server host name</param>
        /// <param name="serverPort">Server port</param>
        /// <param name="useHttps">Whether to use the HTTPS protocol or not</param>
        /// <param name="streamType">Stream type</param>
        internal ShoutcastServer(string serverHost, int serverPort, bool useHttps, string streamType)
        {
            // Check to see if we're dealing with the secure Shoutcast server
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
            serverVersion = ShoutcastVersion.v2;
            mimeType = streamType;
        }

        /// <summary>
        /// Refreshes the statistics
        /// </summary>
        public void Refresh()
        {
            try
            {
                InitializeStatsAsync().Wait();

                // Determine version of Shoutcast
                if (serverVersion == ShoutcastVersion.v1)
                    FinalizeShoutcastV1();
                else
                    FinalizeShoutcastV2();
            }
            catch (Exception ex)
            {
                throw new BasoliaMiscException("Failed to parse radio server {0}. More information can be found in the inner exception.".FormatString(ServerHost), ex);
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

                // Determine version of Shoutcast
                if (serverVersion == ShoutcastVersion.v1)
                    FinalizeShoutcastV1();
                else
                    FinalizeShoutcastV2();
            }
            catch (Exception ex)
            {
                throw new BasoliaMiscException("Failed to parse radio server {0}. More information can be found in the inner exception.".FormatString(ServerHost), ex);
            }
        }

        internal async Task InitializeStatsAsync()
        {
            // Use the full address to download the statistics. Note that Shoutcast v2 streams will use the /statistics directory, which provides
            // more information than /7.html. If we're dealing with the first version, or if /statistics is disabled for some reason, fallback to
            // /7.html
            Uri statisticsUri = new(ServerHostFull + "/statistics?json=1");
            Uri fallbackUri = new(ServerHostFull + "/7.html");
            string serverResponse = await RadioTools.client.GetStringAsync(statisticsUri).ConfigureAwait(false);

            // Shoutcast v1.x doesn't have /statistics...
            if (serverResponse.Contains("Invalid resource"))
            {
                // Detected v1. Fallback to /7.html
                serverVersion = ShoutcastVersion.v1;
                serverResponse = await RadioTools.client.GetStringAsync(fallbackUri).ConfigureAwait(false);
                streamHtmlToken.LoadHtml(serverResponse);
            }
            else
                streamToken = JToken.Parse(serverResponse);
        }

        internal void FinalizeShoutcastV1()
        {
            // Shoutcast version v1.x, so use the html fallback token. Response values are as follows:
            // currentlisteners,streamstatus(S),peaklisteners,maxlisteners,uniquelisteners,bitrate(S),songtitle(S)
            // First, deal with the server settings.
            string[] response = streamHtmlToken.DocumentNode.SelectSingleNode("body").InnerText.Split(',');
            currentListeners = Convert.ToInt32(response[0]);
            peakListeners = Convert.ToInt32(response[2]);
            maxListeners = Convert.ToInt32(response[3]);
            uniqueListeners = Convert.ToInt32(response[4]);

            // Then, deal with the stream settings
            StreamInfo streamInfo = new(this, null);
            streams.Clear();
            streams.Add(streamInfo);
        }

        internal void FinalizeShoutcastV2()
        {
            // Shoutcast version v2.x, so use the JToken.
            // Use all the keys in the first object except the "streams" and "version", where we'd later use the former in StreamInfo to install
            // all the streams into the new class instance.
            if (streamToken is null)
                throw new BasoliaMiscException("Shoutcast v2.x stream token is null");
            totalStreams = (int?)streamToken["totalstreams"] ?? 0;
            activeStreams = (int?)streamToken["activestreams"] ?? 0;
            currentListeners = (int?)streamToken["currentlisteners"] ?? 0;
            peakListeners = (int?)streamToken["peaklisteners"] ?? 0;
            maxListeners = (int?)streamToken["maxlisteners"] ?? 0;
            uniqueListeners = (int?)streamToken["uniquelisteners"] ?? 0;
            averageTime = (int?)streamToken["averagetime"] ?? 0;

            // Now, deal with the stream settings.
            streams.Clear();
            var listStreams = streamToken["streams"] ??
                throw new BasoliaMiscException("There are no streams.");
            foreach (JToken stream in listStreams)
            {
                StreamInfo streamInfo = new(this, stream);
                streams.Add(streamInfo);
            }
        }
    }
}
