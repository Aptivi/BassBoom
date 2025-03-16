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

using BassBoom.Basolia.File;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BassBoom.Basolia.Exceptions;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Basolia.Helpers;

namespace BassBoom.Basolia.Playback
{
    /// <summary>
    /// Playback tools
    /// </summary>
    public static class PlaybackTools
    {
        /// <summary>
        /// Checks to see whether the music is playing
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static bool IsPlaying(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.state == PlaybackState.Playing;
        }

        /// <summary>
        /// The current state of the playback
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static PlaybackState GetState(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.state;
        }

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static string GetRadioIcy(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return basolia.radioIcy;
        }

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static string GetRadioNowPlaying(BasoliaMedia? basolia)
        {
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            string icy = GetRadioIcy(basolia);
            if (icy.Length == 0 || !FileTools.IsRadioStation(basolia))
                return "";
            icy = Regex.Match(icy, @"StreamTitle='(.+?(?=\';))'").Groups[1].Value.Trim().Replace("\\'", "'");
            return icy;
        }

        /// <summary>
        /// Plays the currently open file (synchronous)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaException"></exception>
        public static void Play(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't play a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Now, buffer the entire music file and create an empty array based on its size
                var bufferSize = AudioInfoTools.GetBufferSize(basolia);
                Debug.WriteLine($"Buffer size is {bufferSize}");
                MpvPropertyHandler.SetStringProperty(basolia, "pause", "no");
                string pausing;
                basolia.state = PlaybackState.Playing;
                do
                {
                    // First, let Basolia "hold on" until hold is released
                    while (basolia.holding)
                        Thread.Sleep(1);

                    // Now, check for pause state
                    pausing = MpvPropertyHandler.GetStringProperty(basolia, "pause");
                } while (pausing != "yes" && IsPlaying(basolia));
                if (IsPlaying(basolia) || basolia.state == PlaybackState.Stopping)
                    basolia.state = PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Plays the currently open file (asynchronous)
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static async Task PlayAsync(BasoliaMedia? basolia) =>
            await Task.Run(() => Play(basolia));

        /// <summary>
        /// Pauses the currently open file
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public static void Pause(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't pause a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);
            basolia.state = PlaybackState.Paused;
            MpvPropertyHandler.SetStringProperty(basolia, "pause", "yes");
        }

        /// <summary>
        /// Stops the playback
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <exception cref="BasoliaException"></exception>
        public static void Stop(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check to see if the file is open
            if (!FileTools.IsOpened(basolia))
                throw new BasoliaException("Can't stop a file that's not open", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Stop the music and seek to the beginning
            basolia.state = basolia.state == PlaybackState.Playing ? PlaybackState.Stopping : PlaybackState.Stopped;
            MpvPropertyHandler.SetStringProperty(basolia, "pause", "yes");
            SpinWait.SpinUntil(() => basolia.state == PlaybackState.Stopped);
            if (!FileTools.IsRadioStation(basolia))
                PlaybackPositioningTools.SeekToTheBeginning(basolia);
        }

        /// <summary>
        /// Sets the volume of this application
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="volume">Volume from 0.0 to 1.0 (volume booster off) or 3.0 (volume booster on), inclusive</param>
        /// <param name="volBoost">Whether to allow volumes larger than 1.0 up to 3.0</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetVolume(BasoliaMedia? basolia, double volume, bool volBoost = false)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Check the volume
            double maxVolume = volBoost ? 3 : 1;
            if (volume < 0)
                volume = 0;
            if (volume > maxVolume)
                volume = maxVolume;

            // TODO: Unstub this function
        }

        /// <summary>
        /// Gets the volume information
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>A base linear volume from 0.0 to 1.0, an actual linear volume from 0.0 to 1.0, and the RVA volume in decibels (dB)</returns>
        /// <exception cref="BasoliaException"></exception>
        public static (double baseLinear, double actualLinear, double decibelsRva) GetVolume(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            double baseLinearAddr = 0;
            double actualLinearAddr = 0;
            double decibelsRvaAddr = 0;

            // TODO: Unstub this function
            return (baseLinearAddr, actualLinearAddr, decibelsRvaAddr);
        }

        internal static void FeedRadio(BasoliaMedia? basolia)
        {
            if (!FileTools.IsOpened(basolia) || !FileTools.IsRadioStation(basolia))
                return;
            var currentRadio = FileTools.CurrentFile(basolia);
            if (currentRadio is null)
                return;
            if (currentRadio.Headers is null)
                return;
            if (currentRadio.Stream is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            unsafe
            {
                // Get the MP3 frame length first
                string metaIntStr = currentRadio.Headers.GetValues("icy-metaint").First();
                int metaInt = int.Parse(metaIntStr);

                // Now, get the MP3 frame
                byte[] buffer = new byte[metaInt];
                int numBytesRead = 0;
                int numBytesToRead = metaInt;
                do
                {
                    int n = currentRadio.Stream.Read(buffer, numBytesRead, 1);
                    numBytesRead += n;
                    numBytesToRead -= n;
                } while (numBytesToRead > 0);

                // Fetch the metadata.
                int lengthOfMetaData = currentRadio.Stream.ReadByte();
                int metaBytesToRead = lengthOfMetaData * 16;
                Debug.WriteLine($"Buffer: {lengthOfMetaData} [{metaBytesToRead}]");
                byte[] metadataBytes = new byte[metaBytesToRead];
                currentRadio.Stream.Read(metadataBytes, 0, metaBytesToRead);
                string icy = Encoding.UTF8.GetString(metadataBytes).Replace("\0", "").Trim();
                if (!string.IsNullOrEmpty(icy))
                    basolia.radioIcy = icy;
                Debug.WriteLine($"{basolia.radioIcy}");

                // Copy the data to libmpv
                CopyBuffer(basolia, buffer);
            }
        }

        internal static async Task FeedStream(BasoliaMedia? basolia)
        {
            if (!FileTools.IsOpened(basolia) || FileTools.IsRadioStation(basolia))
                return;
            var currentStream = FileTools.CurrentFile(basolia);
            if (currentStream is null)
                return;
            if (currentStream.Stream is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // Now, get the MP3 frame
            byte[] buffer = new byte[currentStream.Stream.Length];
            await currentStream.Stream.ReadAsync(buffer, 0, (int)currentStream.Stream.Length);

            // Copy the data to libmpv
            CopyBuffer(basolia, buffer);
        }

        internal static void CopyBuffer(BasoliaMedia? basolia, byte[]? buffer)
        {
            if (buffer is null)
                return;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            unsafe
            {
                var handle = basolia._libmpvHandle;

                // Copy the data to libmpv
                IntPtr data = Marshal.AllocHGlobal(buffer.Length);
                Marshal.Copy(buffer, 0, data, buffer.Length);

                // TODO: Unstub this function
            }
        }

        internal static int PlayBuffer(BasoliaMedia? basolia, byte[]? buffer)
        {
            if (buffer is null)
                return 0;
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // TODO: Unstub this function
            return 0;
        }
    }
}
