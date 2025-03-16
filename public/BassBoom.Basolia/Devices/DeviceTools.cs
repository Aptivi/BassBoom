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

using BassBoom.Basolia.Exceptions;
using BassBoom.Native.Interop.Enumerations;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BassBoom.Basolia.Devices
{
    /// <summary>
    /// Device and driver tools
    /// </summary>
    public static class DeviceTools
    {
        /// <summary>
        /// Gets a read only dictionary that lists all the drivers
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>A dictionary containing the driver names and their descriptions</returns>
        /// <exception cref="BasoliaException"></exception>
        public static ReadOnlyDictionary<string, string> GetDrivers(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            Dictionary<string, string> drivers = [];

            // TODO: Unstub this function
            return new ReadOnlyDictionary<string, string>(drivers);
        }

        /// <summary>
        /// Gets a read only dictionary that lists all the devices detected by the driver
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="driver">A specific driver to use</param>
        /// <param name="activeDevice">An output for the active device name</param>
        /// <returns>A dictionary containing the device names and their descriptions</returns>
        /// <exception cref="BasoliaException"></exception>
        public static ReadOnlyDictionary<string, string> GetDevices(BasoliaMedia? basolia, string driver, ref string activeDevice)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            Dictionary<string, string> devices = [];

            // TODO: Unstub this function
            return new ReadOnlyDictionary<string, string>(devices);
        }

        /// <summary>
        /// Gets the current device and driver
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Current device and driver</returns>
        /// <exception cref="BasoliaException"></exception>
        public static (string driver, string device) GetCurrent(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // TODO: Unstub this function
            return ("", "");
        }

        /// <summary>
        /// Gets the current cached device and driver
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <returns>Current cached device and driver</returns>
        /// <exception cref="BasoliaException"></exception>
        public static (string? driver, string? device) GetCurrentCached(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            return (basolia.activeDriver, basolia.activeDevice);
        }

        /// <summary>
        /// Sets the active driver
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="driver">Driver to use</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetActiveDriver(BasoliaMedia? basolia, string driver)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            var driverList = GetDrivers(basolia);
            if (!driverList.ContainsKey(driver))
                throw new BasoliaException($"Driver {driver} doesn't exist", MpvError.MPV_ERROR_INVALID_PARAMETER);
            basolia.activeDriver = driver;
        }

        /// <summary>
        /// Sets the active device
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="driver">Driver to use</param>
        /// <param name="device">Device to use</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetActiveDevice(BasoliaMedia? basolia, string driver, string device)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            basolia.activeDevice = "";
            var deviceList = GetDevices(basolia, driver, ref basolia.activeDevice);
            if (string.IsNullOrEmpty(device))
                return;
            if (!deviceList.ContainsKey(device))
                throw new BasoliaException($"Device {device} doesn't exist", MpvError.MPV_ERROR_INVALID_PARAMETER);
            basolia.activeDevice = device;
        }

        /// <summary>
        /// Resets the driver and the device selection to their initial settings
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        public static void Reset(BasoliaMedia? basolia)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);
            basolia.activeDriver = null;
            basolia.activeDevice = null;
        }
    }
}
