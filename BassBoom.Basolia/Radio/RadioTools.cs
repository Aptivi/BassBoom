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

        public static IRadioServer GetRadioInfo(string radioUrl) =>
            GetRadioInfoAsync(radioUrl).Result;

        public static async Task<IRadioServer> GetRadioInfoAsync(string radioUrl)
        {
            // Check to see if we provided a path
            if (string.IsNullOrEmpty(radioUrl))
                throw new BasoliaMiscException("Provide a path to a radio station");
            var uri = new Uri(radioUrl);

            // Check to see if the radio station exists
            client.DefaultRequestHeaders.Add("Icy-MetaData", "1");
            var reply = await client.GetAsync(radioUrl, HttpCompletionOption.ResponseHeadersRead);
            client.DefaultRequestHeaders.Remove("Icy-MetaData");
            if (!reply.IsSuccessStatusCode)
                throw new BasoliaMiscException($"This radio station doesn't exist. Error code: {(int)reply.StatusCode} ({reply.StatusCode}).");

            // Check to see if there are any ICY headers
            if (!reply.Headers.Any((kvp) => kvp.Key.StartsWith("icy-")))
                throw new BasoliaMiscException("This doesn't look like a radio station. Are you sure?");
            
            // Now, check the server type
            RadioServerType type = RadioServerType.Unknown;
            if (reply.Headers.Server.ToString().Equals("icecast", StringComparison.OrdinalIgnoreCase))
                type = RadioServerType.Icecast;
            else if (reply.Headers.Contains("icy-notice2") && reply.Headers.GetValues("icy-notice2").First().ToLower().Contains("shoutcast"))
                type = RadioServerType.Shoutcast;
            else
                throw new BasoliaMiscException("Can't determine radio station server type.");

            // Return the appropriate parsed server stats instance
            return type switch
            {
                RadioServerType.Shoutcast =>
                    new ShoutcastServer(uri.Host, uri.Port, uri.Scheme == "https"),
                RadioServerType.Icecast =>
                    new IcecastServer(uri.Host, uri.Port, uri.Scheme == "https"),
                _ => null,
            };
        }
    }
}
