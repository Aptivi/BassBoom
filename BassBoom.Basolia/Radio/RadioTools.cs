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

using BassBoom.Basolia.Exceptions;
using SpecProbe.Software.Platform;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// Radio tools
    /// </summary>
    public static class RadioTools
    {
        internal static HttpClient client = new();

        /// <summary>
        /// Gets extended radio station information
        /// </summary>
        /// <param name="radioUrl">Radio station URL</param>
        /// <returns>An instance of <see cref="IRadioServer"/>. <see langword="null"/> if type can't be determined. <see cref="ShoutcastServer"/> if this radio station server uses Shoutcast, and <see cref="IcecastServer"/> if it uses Icecast.</returns>
        public static IRadioServer? GetRadioInfo(string radioUrl) =>
            Task.Run(() => GetRadioInfoAsync(radioUrl)).GetAwaiter().GetResult();

        /// <summary>
        /// Gets extended radio station information asynchronously
        /// </summary>
        /// <param name="radioUrl">Radio station URL</param>
        /// <returns>An instance of <see cref="IRadioServer"/>. <see langword="null"/> if type can't be determined. <see cref="ShoutcastServer"/> if this radio station server uses Shoutcast, and <see cref="IcecastServer"/> if it uses Icecast.</returns>
        public static async Task<IRadioServer?> GetRadioInfoAsync(string radioUrl)
        {
            // Check to see if we provided a path
            if (string.IsNullOrEmpty(radioUrl))
                throw new BasoliaMiscException("Provide a path to a radio station");
            var uri = new Uri(radioUrl);

            // Check to see if the radio station exists
            if (PlatformHelper.IsDotNetFx())
                client = new();
            client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await client.GetAsync(radioUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaMiscException($"This radio station doesn't exist. Error code: {(int)reply.StatusCode} ({reply.StatusCode}).");

            // Check for radio statio and get the MIME type
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaMiscException("This doesn't look like a radio station. Are you sure?");
            var contentType = reply.Content.Headers.ContentType;
            string streamType = contentType.MediaType;

            // Now, check the server type
            RadioServerType type = RadioServerType.Unknown;
            if (reply.Headers.Server.ToString().ToLower().Contains("icecast"))
                type = RadioServerType.Icecast;
            else if (reply.Headers.Contains("icy-notice2") && reply.Headers.GetValues("icy-notice2").First().ToLower().Contains("shoutcast"))
                type = RadioServerType.Shoutcast;
            else
                throw new BasoliaMiscException("Can't determine radio station server type.");

            // Return the appropriate parsed server stats instance
            IRadioServer? stats = type switch
            {
                RadioServerType.Shoutcast =>
                    new ShoutcastServer(uri.Host, uri.Port, uri.Scheme == "https", streamType),
                RadioServerType.Icecast =>
                    new IcecastServer(uri.Host, uri.Port, uri.Scheme == "https", streamType),
                _ => null,
            };
            if (stats is null)
                return null;
            await stats.RefreshAsync().ConfigureAwait(false);
            return stats;
        }

        /// <summary>
        /// Checks to see if this is a radio station or not
        /// </summary>
        /// <param name="radioUrl">Radio station URL</param>
        /// <param name="streamType">Output stream type</param>
        /// <returns>True if it is a radio station; false otherwise.</returns>
        public static bool IsRadio(string radioUrl, out string streamType)
        {
            var result = Task.Run(() => IsRadioAsync(radioUrl)).GetAwaiter().GetResult();
            streamType = result.Item1;
            return result.Item2;
        }

        /// <summary>
        /// Checks to see if this is a radio station or not
        /// </summary>
        /// <param name="radioUrl">Radio station URL</param>
        /// <returns>True if it is a radio station; false otherwise.</returns>
        public static async Task<(string, bool)> IsRadioAsync(string radioUrl)
        {
            // Check to see if we provided a path
            if (string.IsNullOrEmpty(radioUrl))
                throw new BasoliaMiscException("Provide a path to a radio station");
            var uri = new Uri(radioUrl);

            // Check to see if the radio station exists
            if (PlatformHelper.IsDotNetFx())
                client = new();
            client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await client.GetAsync(radioUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaMiscException($"This radio station doesn't exist. Error code: {(int)reply.StatusCode} ({reply.StatusCode}).");

            // Check for radio statio and get the MIME type
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaMiscException("This doesn't look like a radio station. Are you sure?");
            var contentType = reply.Content.Headers.ContentType;
            string streamType = contentType.MediaType;

            // Now, check the server type
            if (reply.Headers.Server.ToString().ToLower().Contains("icecast"))
                return (streamType, true);
            else if (reply.Headers.Contains("icy-notice2") && reply.Headers.GetValues("icy-notice2").First().ToLower().Contains("shoutcast"))
                return (streamType, true);
            else
                return (streamType, false);
        }
    }
}
