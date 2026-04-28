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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BassBoom.Basolia.Exceptions;
using BassBoom.Basolia.Languages;
using BassBoom.Basolia.Media.Enumerations;
using BassBoom.Basolia.Media.Format;
using BassBoom.Basolia.Media.Playback;
using BassBoom.Native;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Interop.Play;
using Textify.General;

namespace BassBoom.Basolia.Media
{
    /// <summary>
    /// Basolia instance for media manipulation
    /// </summary>
    public partial class BasoliaMedia
    {
        #region Positioning tools
        internal object PositionLock = new();

        /// <summary>
        /// Gets the current duration of the file (samples)
        /// </summary>
        /// <returns>Current duration in samples</returns>
        public int GetCurrentDuration()
        {
            int length;
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_FORMAT_EXCEPTION_FILENOTOPEN_QUERY"), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                var handle = _mpg123Handle;

                // Get the length
                var @delegate = MpgNative.GetDelegate<NativePositioning.mpg123_tell>(MpgNative.libManagerMpg, nameof(NativePositioning.mpg123_tell));
                length = @delegate.Invoke(handle);
                if (length == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_DURATIONFAILED"), mpg123_errors.MPG123_ERR);
            }

            // We're now entering the safe zone
            return length;
        }

        /// <summary>
        /// Gets the current duration of the file (time span)
        /// </summary>
        /// <returns>A time span instance that describes the current duration of the file</returns>
        public TimeSpan GetCurrentDurationSpan()
        {
            InitBasolia.CheckInited();

            // First, get the format information
            var formatInfo = GetFormatInfo();

            // Get the required values
            long rate = formatInfo.rate;
            int durationSamples = GetCurrentDuration();
            long seconds = durationSamples / rate;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Seeks to the beginning of the music
        /// </summary>
        public void SeekToTheBeginning()
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
    
                // Check to see if the file is open
                if (!IsOpened())
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_FILENOTOPEN_SEEK"), mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var handle = _mpg123Handle;
                    var outHandle = _out123Handle;

                    // Get the length
                    holding = true;
                    while (bufferPlaying)
                        Thread.Sleep(1);
                    Drop();
                    var @delegate = MpgNative.GetDelegate<NativePositioning.mpg123_seek>(MpgNative.libManagerMpg, nameof(NativePositioning.mpg123_seek));
                    int status = @delegate.Invoke(handle, 0, 0);
                    holding = false;
                    if (status == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_SEEKTOSTARTFAILED"), mpg123_errors.MPG123_LSEEK_FAILED);
                }
            }
        }

        /// <summary>
        /// Seeks to a specific frame
        /// </summary>
        /// <param name="frame">An MPEG frame number</param>
        public void SeekToFrame(int frame)
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
    
                // Check to see if the file is open
                if (!IsOpened())
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_FILENOTOPEN_SEEK"), mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var handle = _mpg123Handle;
                    var outHandle = _out123Handle;

                    // Get the length
                    holding = true;
                    while (bufferPlaying)
                        Thread.Sleep(1);
                    Drop();
                    var @delegate = MpgNative.GetDelegate<NativePositioning.mpg123_seek>(MpgNative.libManagerMpg, nameof(NativePositioning.mpg123_seek));
                    int status = @delegate.Invoke(handle, frame, 0);
                    holding = false;
                    if (status == (int)mpg123_errors.MPG123_ERR)
                        throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_SEEKTOFRAMEFAILED").FormatString(frame), (mpg123_errors)status);
                }
            }
        }

        /// <summary>
        /// Drops all MPEG frames to the device
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void Drop()
        {
            lock (PositionLock)
            {
                InitBasolia.CheckInited();
    
                // Check to see if the file is open
                if (!IsOpened())
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_DROPFAILED"), mpg123_errors.MPG123_BAD_FILE);

                // We're now entering the dangerous zone
                unsafe
                {
                    var outHandle = _out123Handle;
                    var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_drop>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_drop));
                    @delegate.Invoke(outHandle);
                }
            }
        }
        #endregion

        #region Playback tools
        /// <summary>
        /// Checks to see whether the music is playing
        /// </summary>
        public bool IsPlaying() =>
            state == PlaybackState.Playing;

        /// <summary>
        /// The current state of the playback
        /// </summary>
        public PlaybackState GetState() =>
            state;

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        public string GetRadioIcy() =>
            radioIcy;

        /// <summary>
        /// Current radio ICY metadata
        /// </summary>
        public string GetRadioNowPlaying()
        {
            string icy = GetRadioIcy();
            if (icy.Length == 0 || !IsRadioStation())
                return "";
            icy = Regex.Match(icy, @"StreamTitle='(.+?(?=\';))'").Groups[1].Value.Trim().Replace("\\'", "'");
            return icy;
        }

        /// <summary>
        /// Plays the currently open file (synchronous)
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        /// <exception cref="BasoliaOutException"></exception>
        public void Play()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_FILENOTOPEN_PLAY"), mpg123_errors.MPG123_BAD_FILE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Reset the format. Orders here matter.
                var (rate, channels, encoding) = GetFormatInfo();
                NoFormat();

                // Set the format
                UseFormat(rate, (ChannelCount)channels, encoding);
                Debug.WriteLine($"Format {rate}, {channels}, {encoding}");

                // Try to open output to device
                OpenOutput();

                // Start the output
                StartOutput(rate, (ChannelCount)channels, encoding);

                // Now, buffer the entire music file and create an empty array based on its size
                var bufferSize = GetBufferSize();
                Debug.WriteLine($"Buffer size is {bufferSize}");
                int err;
                state = PlaybackState.Playing;
                do
                {
                    int num = 0;
                    int audioBytes = 0;
                    byte[]? audio = null;

                    // First, let Basolia "hold on" until hold is released
                    while (holding)
                        Thread.Sleep(1);

                    // Now, play the MPEG buffer to the device
                    bufferPlaying = true;
                    err = DecodeFrame(ref num, ref audio, ref audioBytes);
                    PlayBuffer(audio);
                    bufferPlaying = false;

                    // Check to see if we need more (radio)
                    if (IsRadioStation() && err == (int)mpg123_errors.MPG123_NEED_MORE)
                    {
                        err = (int)mpg123_errors.MPG123_OK;
                        FeedRadio();
                    }
                } while (err == (int)mpg123_errors.MPG123_OK && IsPlaying());
                if (IsPlaying() || state == PlaybackState.Stopping)
                    state = PlaybackState.Stopped;
            }
        }

        /// <summary>
        /// Plays the currently open file (asynchronous)
        /// </summary>
        public async Task PlayAsync() =>
            await Task.Run(() => Play());

        /// <summary>
        /// Pauses the currently open file
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void Pause()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_FILENOTOPEN_PAUSE"), mpg123_errors.MPG123_BAD_FILE);
            state = PlaybackState.Paused;
        }

        /// <summary>
        /// Stops the playback
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void Stop()
        {
            InitBasolia.CheckInited();

            // Check to see if the file is open
            if (!IsOpened())
                throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_FILENOTOPEN_STOP"), mpg123_errors.MPG123_BAD_FILE);

            // Stop the music and seek to the beginning
            state = state == PlaybackState.Playing ? PlaybackState.Stopping : PlaybackState.Stopped;
            SpinWait.SpinUntil(() => state == PlaybackState.Stopped);
            if (!IsRadioStation())
                SeekToTheBeginning();
        }

        /// <summary>
        /// Sets the volume of this application
        /// </summary>
        /// <param name="volume">Volume from 0.0 to 1.0 (volume booster off) or 3.0 (volume booster on), inclusive</param>
        /// <param name="volBoost">Whether to allow volumes larger than 1.0 up to 3.0</param>
        /// <exception cref="BasoliaOutException"></exception>
        public void SetVolume(double volume, bool volBoost = false)
        {
            InitBasolia.CheckInited();

            // Check the volume
            double maxVolume = volBoost ? 3 : 1;
            if (volume < 0)
                volume = 0;
            if (volume > maxVolume)
                volume = maxVolume;

            // Try to set the volume
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_volume>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_volume));
                int status = @delegate.Invoke(handle, volume);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_SETVOLUMEFAILED").FormatString(volume), (out123_error)status);
            }
        }

        /// <summary>
        /// Gets the volume information
        /// </summary>
        /// <returns>A base linear volume from 0.0 to 1.0, an actual linear volume from 0.0 to 1.0, and the RVA volume in decibels (dB)</returns>
        /// <exception cref="BasoliaOutException"></exception>
        public (double baseLinear, double actualLinear, double decibelsRva) GetVolume()
        {
            InitBasolia.CheckInited();

            double baseLinearAddr = 0;
            double actualLinearAddr = 0;
            double decibelsRvaAddr = 0;

            // Try to get the volume
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_getvolume>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_getvolume));
                int status = @delegate.Invoke(handle, ref baseLinearAddr, ref actualLinearAddr, ref decibelsRvaAddr);
                if (status != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_GETVOLUMEFAILED"), (out123_error)status);
            }

            // Get the volume information
            return (baseLinearAddr, actualLinearAddr, decibelsRvaAddr);
        }

        /// <summary>
        /// Sets the equalizer band to any value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public void SetEqualizer(PlaybackChannels channels, int bandIdx, double value)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_eq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_eq));
                int status = @delegate.Invoke(handle, (mpg123_channels)channels, bandIdx, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_SETEQBANDFAILED").FormatString(bandIdx + 1, value, channels), (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Sets the equalizer bands to any value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdxStart">Band index from 0 to 31 (first band to start from)</param>
        /// <param name="bandIdxEnd">Band index from 0 to 31 (second band to end to)</param>
        /// <param name="value">Value of the equalizer</param>
        /// <exception cref="BasoliaException"></exception>
        public void SetEqualizerRange(PlaybackChannels channels, int bandIdxStart, int bandIdxEnd, double value)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_eq_bands>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_eq_bands));
                int status = @delegate.Invoke(handle, (int)channels, bandIdxStart, bandIdxEnd, value);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_SETEQRANGEBANDFAILED").FormatString(bandIdxStart + 1, bandIdxEnd + 1, value, channels), (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the equalizer band value
        /// </summary>
        /// <param name="channels">Mono, stereo, or both</param>
        /// <param name="bandIdx">Band index from 0 to 31</param>
        /// <exception cref="BasoliaException"></exception>
        public double GetEqualizer(PlaybackChannels channels, int bandIdx)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_geteq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_geteq));
                double eq = @delegate.Invoke(handle, (mpg123_channels)channels, bandIdx);
                return eq;
            }
        }

        /// <summary>
        /// Resets the equalizer band to its natural value
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void ResetEqualizer()
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeVolume.mpg123_reset_eq>(MpgNative.libManagerMpg, nameof(NativeVolume.mpg123_reset_eq));
                int status = @delegate.Invoke(handle);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_RESETEQBANDSFAILED"), (mpg123_errors)status);
            }
        }

        /// <summary>
        /// Gets the native state
        /// </summary>
        /// <param name="state">A native state to get</param>
        /// <returns>A number that represents the value of this state</returns>
        /// <exception cref="BasoliaException"></exception>
        public (long, double) GetNativeState(PlaybackStateType state)
        {
            InitBasolia.CheckInited();

            // Try to set the equalizer value
            unsafe
            {
                long stateInt = 0;
                double stateDouble = 0;
                var handle = _mpg123Handle;
                var @delegate = MpgNative.GetDelegate<NativeStatus.mpg123_getstate>(MpgNative.libManagerMpg, nameof(NativeStatus.mpg123_getstate));
                int status = @delegate.Invoke(handle, (mpg123_state)state, ref stateInt, ref stateDouble);
                if (status != (int)mpg123_errors.MPG123_OK)
                    throw new BasoliaException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_GETNATIVESTATEFAILED").FormatString(state.ToString()), (mpg123_errors)status);
                return (stateInt, stateDouble);
            }
        }

        /// <summary>
        /// Opens the output to the device. You can set your preferred driver and device using <see cref="SetActiveDriver(string)"/> and <see cref="SetActiveDevice(string, string)"/>.
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void OpenOutput()
        {
            InitBasolia.CheckInited();
            if (isOutputOpen)
                return;

            // Try to open output to device
            unsafe
            {
                var outHandle = _out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_open>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_open));
                int openStatus = @delegate.Invoke(outHandle, activeDriver, activeDevice);
                if (openStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_OPENOUTPUTFAILED").FormatString(activeDevice, activeDriver), (out123_error)openStatus);
                isOutputOpen = true;
            }
        }

        /// <summary>
        /// Opens the output to the device. You can set your preferred driver and device using <see cref="SetActiveDriver(string)"/> and <see cref="SetActiveDevice(string, string)"/>.
        /// </summary>
        /// <param name="rate">Rate to open. It must be supported and must match what you've set in <see cref="UseFormat(long, ChannelCount, int)"/></param>
        /// <param name="channels">Channels to open. They must be supported and must match what you've set in <see cref="UseFormat(long, ChannelCount, int)"/></param>
        /// <param name="encoding">Encoding to open. It must be supported and must match what you've set in <see cref="UseFormat(long, ChannelCount, int)"/></param>
        /// <exception cref="BasoliaException"></exception>
        public void StartOutput(long rate, ChannelCount channels, int encoding)
        {
            InitBasolia.CheckInited();

            // Sanity checks
            bool supported = IsFormatSupported(rate, encoding, out _);
            if (!isOutputOpen)
                throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_NEEDSOUTPUT"), out123_error.OUT123_NOT_LIVE);
            if (!supported)
                throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_RATEENCODINGUNSUPPORTED").FormatString(rate, encoding), out123_error.OUT123_NOT_SUPPORTED);

            // Try to open output to device
            unsafe
            {
                var outHandle = _out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_start>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_start));
                int startStatus = @delegate.Invoke(outHandle, rate, (int)channels, encoding);
                if (startStatus != (int)out123_error.OUT123_OK)
                    throw new BasoliaOutException(LanguageTools.GetLocalized("BASSBOOM_BASOLIA_PLAYBACK_EXCEPTION_STARTOUTPUTFAILED"), (out123_error)startStatus);
            }
        }

        /// <summary>
        /// Closes the output to the device.
        /// </summary>
        /// <exception cref="BasoliaException"></exception>
        public void CloseOutput()
        {
            InitBasolia.CheckInited();
            if (!isOutputOpen)
                return;

            // Try to open output to device
            unsafe
            {
                var outHandle = _out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_stop>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_stop));
                @delegate.Invoke(outHandle);
                var delegate2 = MpgNative.GetDelegate<NativeOutputLib.out123_close>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_close));
                delegate2.Invoke(outHandle);
                isOutputOpen = false;
            }
        }
        #endregion
    }
}
