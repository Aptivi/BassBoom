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
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace BassBoom.Basolia.Radio
{
    /// <summary>
    /// Shoutcast stream information
    /// </summary>
    [DebuggerDisplay("{StreamTitle,nq} is now playing {SongTitle} | L[{CurrentListeners}]")]
    public class StreamInfo
    {
        private readonly int currentListeners;
        private readonly int peakListeners;
        private readonly int maxListeners;
        private readonly int uniqueListeners;
        private readonly int averageTime;
        private readonly int streamId;
        private readonly string streamGenre;
        private readonly string streamGenre2;
        private readonly string streamGenre3;
        private readonly string streamGenre4;
        private readonly string streamGenre5;
        private readonly string streamHomepage;
        private readonly string streamTitle;
        private readonly string songTitle;
        private readonly long streamHits;
        private readonly int streamStatus;
        private readonly int backupStatus;
        private readonly bool streamListed;
        private readonly string streamPath;
        private readonly long streamUptime;
        private readonly int bitRate;
        private readonly int sampleRate;
        private readonly string mimeInfo;

        /// <summary>
        /// Stream ID starting from number one (1)
        /// </summary>
        public int StreamId => streamId;
        /// <summary>
        /// How many people are listening to the stream at this time?
        /// </summary>
        public int CurrentListeners => currentListeners;
        /// <summary>
        /// How many listeners did the stream ever get at peak times?
        /// </summary>
        public int PeakListeners => peakListeners;
        /// <summary>
        /// How many people can listen to the stream?
        /// </summary>
        public int MaxListeners => maxListeners;
        /// <summary>
        /// How many unique listeners are there?
        /// </summary>
        public int UniqueListeners => uniqueListeners;
        /// <summary>
        /// Average time on any active listener connections in seconds
        /// </summary>
        public int AverageTime => averageTime;
        /// <summary>
        /// Average time on any active listener connections in the time span
        /// </summary>
        public TimeSpan AverageTimeSpan => TimeSpan.FromSeconds(averageTime);
        /// <summary>
        /// The stream genre
        /// </summary>
        public string StreamGenre => streamGenre;
        /// <summary>
        /// The second stream genre
        /// </summary>
        public string StreamGenre2 => streamGenre2;
        /// <summary>
        /// The third stream genre
        /// </summary>
        public string StreamGenre3 => streamGenre3;
        /// <summary>
        /// The fourth stream genre
        /// </summary>
        public string StreamGenre4 => streamGenre4;
        /// <summary>
        /// The fifth stream genre
        /// </summary>
        public string StreamGenre5 => streamGenre5;
        /// <summary>
        /// Link to the stream homepage
        /// </summary>
        public string StreamHomepage => streamHomepage;
        /// <summary>
        /// Stream title
        /// </summary>
        public string StreamTitle => streamTitle;
        /// <summary>
        /// Song title
        /// </summary>
        public string SongTitle => songTitle;
        /// <summary>
        /// Stream hits
        /// </summary>
        public long StreamHits => streamHits;
        /// <summary>
        /// Stream status
        /// </summary>
        public int StreamStatus => streamStatus;
        /// <summary>
        /// Backup status
        /// </summary>
        public int BackupStatus => backupStatus;
        /// <summary>
        /// Is the stream listed?
        /// </summary>
        public bool StreamListed => streamListed;
        /// <summary>
        /// Path to stream
        /// </summary>
        public string StreamPath => streamPath;
        /// <summary>
        /// Stream uptime in seconds
        /// </summary>
        public long StreamUptime => streamUptime;
        /// <summary>
        /// Stream uptime in the time span
        /// </summary>
        public TimeSpan StreamUptimeSpan => TimeSpan.FromSeconds(StreamUptime);
        /// <summary>
        /// Stream bitrate in kbps
        /// </summary>
        public int BitRate => bitRate;
        /// <summary>
        /// Sampling rate in Hz
        /// </summary>
        public int SampleRate => sampleRate;
        /// <summary>
        /// MIME info for stream, usually audio/mpeg.
        /// </summary>
        public string MimeInfo => mimeInfo;

        /// <summary>
        /// Makes a new stream information which has all the streams
        /// </summary>
        /// <param name="server">Radio server class instance</param>
        /// <param name="stream">Individual stream token</param>
        internal StreamInfo(IRadioServer server, JToken stream)
        {
            try
            {
                if (server is ShoutcastServer shoutcastServer)
                {
                    // Determine version of Shoutcast
                    if (shoutcastServer.ServerVersion == ShoutcastVersion.v1)
                    {
                        // Shoutcast version v1.x, so use the html fallback token.
                        string[] response = shoutcastServer.streamHtmlToken.DocumentNode.SelectSingleNode("body").InnerText.Split(',');
                        currentListeners = Convert.ToInt32(response[0]);
                        streamStatus = Convert.ToInt32(response[1]);
                        peakListeners = Convert.ToInt32(response[2]);
                        maxListeners = Convert.ToInt32(response[3]);
                        uniqueListeners = Convert.ToInt32(response[4]);
                        bitRate = Convert.ToInt32(response[5]);
                        songTitle = response[6];
                    }
                    else
                    {
                        // Shoutcast version v2.x, so use the JToken.
                        streamId = (int)stream["id"];
                        currentListeners = (int)stream["currentlisteners"];
                        peakListeners = (int)stream["peaklisteners"];
                        maxListeners = (int)stream["maxlisteners"];
                        uniqueListeners = (int)stream["uniquelisteners"];
                        averageTime = (int)stream["averagetime"];
                        streamGenre = (string)stream["servergenre"];
                        streamGenre2 = (string)stream["servergenre2"];
                        streamGenre3 = (string)stream["servergenre3"];
                        streamGenre4 = (string)stream["servergenre4"];
                        streamGenre5 = (string)stream["servergenre5"];
                        streamHomepage = (string)stream["serverurl"];
                        streamTitle = (string)stream["servertitle"];
                        songTitle = (string)stream["songtitle"];
                        streamHits = (int)stream["streamhits"];
                        streamStatus = (int)stream["streamstatus"];
                        backupStatus = (int)stream["backupstatus"];
                        streamListed = (bool)stream["streamlisted"];
                        streamPath = (string)stream["streampath"];
                        streamUptime = (int)stream["streamuptime"];
                        bitRate = (int)stream["bitrate"];
                        sampleRate = (int)stream["samplerate"];
                        mimeInfo = (string)stream["content"];
                    }
                }
                else if (server is IcecastServer icecastServer)
                {
                    // Icecast server, so use the JToken.
                    if (stream["dummy"] is not null)
                        return;
                    currentListeners = (int)stream["listeners"];
                    peakListeners = (int)stream["listener_peak"];
                    streamGenre = (string)stream["genre"];
                    streamHomepage = (string)stream["server_url"];
                    streamTitle = (string)stream["server_name"];
                    songTitle = (string)stream["title"];
                    streamListed = true;
                    streamPath = (string)stream["listenurl"];
                    bitRate = (int)stream["bitrate"];
                    mimeInfo = (string)stream["server_type"];
                }
            }
            catch (Exception ex)
            {
                throw new BasoliaMiscException($"Failed to parse stream ID {streamId}. More information can be found in the inner exception.", ex);
            }
        }
    }
}
