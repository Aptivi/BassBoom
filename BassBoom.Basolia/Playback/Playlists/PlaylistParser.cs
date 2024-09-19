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
using BassBoom.Basolia.File;
using BassBoom.Basolia.Playback.Playlists.Enumerations;
using BassBoom.Basolia.Playback.Playlists.Instances;
using BassBoom.Basolia.Radio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Textify.General;
using FileIo = System.IO.File;

namespace BassBoom.Basolia.Playback.Playlists
{
    /// <summary>
    /// Playlist parser that supports M3U and M3U8
    /// </summary>
    public static class PlaylistParser
    {
        /// <summary>
        /// Parses the playlist from a file
        /// </summary>
        /// <param name="playlistFile">Path to a playlist file</param>
        /// <returns>An instance of <see cref="Playlist"/> containing parsed <see cref="TrackInfo"/> instances</returns>
        public static Playlist ParsePlaylist(string playlistFile)
        {
            // Check for existence and extension
            if (!FileIo.Exists(playlistFile))
                throw new BasoliaMiscException($"Playlist file {playlistFile} doesn't exist");
            string extension = Path.GetExtension(playlistFile);
            if (Path.HasExtension(playlistFile) && extension != PlaylistConstants.m3u && extension != PlaylistConstants.m3u8)
                throw new BasoliaMiscException($"Invalid playlist file extension {extension}");

            // Now, try to get the representation and pass it to ParsePlaylistFrom().
            string representation = FileIo.ReadAllText(playlistFile);
            string fileParent = Path.GetDirectoryName(playlistFile);
            return ParsePlaylistFrom(representation, fileParent);
        }

        /// <summary>
        /// Parses the playlist from a representation
        /// </summary>
        /// <param name="playlist">String representation of an m3u playlist</param>
        /// <param name="fileParent">Parent directory of a file that is an m3u playlist</param>
        /// <returns>An instance of <see cref="Playlist"/> containing parsed <see cref="TrackInfo"/> instances</returns>
        public static Playlist ParsePlaylistFrom(string playlist, string fileParent = "")
        {
            // Sanity checks
            fileParent ??= "";
            if (string.IsNullOrEmpty(playlist))
                throw new BasoliaMiscException("Playlist representation is not provided.");
            string[] lines = playlist.SplitNewLines();
            if (lines.Length == 0)
                throw new BasoliaMiscException("Playlist representation is not provided.");

            // Read this playlist line by line
            bool extended = false;
            string trackInfoString = "";
            List<(string trackInfo, Uri? trackPathUri, string trackPath)> proposedTracks = [];
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check for an extended header
                if (line.StartsWith("#") && i == 0)
                {
                    // Check if this is an extended M3U header
                    string header = line.Substring(1);
                    if (header == PlaylistConstants.extendedHeader)
                        extended = true;
                    else
                        throw new BasoliaMiscException($"This [{header}] is not an extended header");
                    continue;
                }

                // Check for track info
                if (extended && line.StartsWith("#"))
                {
                    // Check if this is a track info declaration, and ignore all non-standard properties
                    string declaration = line.Substring(1);
                    if (declaration.StartsWith(PlaylistConstants.extendedInfo))
                    {
                        if (!declaration.Contains(':'))
                            throw new BasoliaMiscException("EXTINF requires exactly two arguments, and there is no argument indicator.");
                        trackInfoString = declaration.Substring(declaration.IndexOf(':') + 1);
                    }
                    continue;
                }

                // Check for MPEG files or URLs
                if (Uri.TryCreate(line, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
                    proposedTracks.Add((trackInfoString, uri, uri.ToString()));
                else
                {
                    string absolutePath = !string.IsNullOrEmpty(fileParent) ? Path.Combine(fileParent, line) : line;
                    string extension = Path.GetExtension(absolutePath);
                    if (FileIo.Exists(absolutePath) && FileTools.SupportedExtensions.Contains(extension))
                        proposedTracks.Add((trackInfoString, null, absolutePath));
                    else
                        throw new BasoliaMiscException($"Music file {absolutePath} not found or not an MPEG file");
                }
                trackInfoString = "";
            }

            // Now, parse the proposed tracks by finalizing them and deserializing them to TrackInfo instances
            List<TrackInfo> tracks = [];
            for (int i = 0; i < proposedTracks.Count; i++)
            {
                // Get the proposed track info and determine the initial type
                (string trackInfo, Uri? trackPathUri, string trackPath) = proposedTracks[i];
                SongType trackType = trackPathUri is not null ? SongType.URL : SongType.File;

                // Now, determine if this URL is a radio station
                string streamType = "";
                bool radio = trackPathUri is not null && RadioTools.IsRadio(trackPath, out streamType);
                trackType = trackType == SongType.URL && radio ? SongType.Radio : trackType;

                // Parse the track info (ex: 123,Artist Name - Track Title) if found
                int length = 0;
                string title = "";
                if (!string.IsNullOrWhiteSpace(trackInfo))
                {
                    if (!trackInfo.Contains(','))
                        throw new BasoliaMiscException("Track info must provide exactly two arguments: track length in seconds (-1 if it's a livestream) and the raw track title.");
                    string lengthStr = trackInfo.Substring(0, trackInfo.IndexOf(','));
                    string titleStr = trackInfo.Substring(trackInfo.IndexOf(','));

                    // Check the validity and parse them
                    if (!int.TryParse(lengthStr, out length))
                        throw new BasoliaMiscException($"Track info didn't provide the number of seconds [{lengthStr}] correctly.");
                    title = titleStr.Length == 1 ? "" : titleStr.Substring(1);
                }

                // Check to see if we have an MPEG stream type (radios)
                if (radio && streamType != "audio/mpeg")
                    continue;

                // Make a new instance of TrackInfo to install its values and to add this instance to the array of tracks
                var trackInfoInstance = new TrackInfo(length, title, trackPath, trackType);
                tracks.Add(trackInfoInstance);
            }

            // Make a new playlist instance
            return new Playlist([.. tracks]);
        }
    }
}
