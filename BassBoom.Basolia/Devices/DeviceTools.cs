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
using BassBoom.Basolia.Helpers;
using BassBoom.Native;
using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            Dictionary<string, string> drivers = [];

            // We're now entering the dangerous zone
            nint names = IntPtr.Zero, descr = IntPtr.Zero;
            int driverCount;
            string[] driverNames, driverDescs;
            unsafe
            {
                // Query the drivers
                var handle = basolia._out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_drivers>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_drivers));
                int driversStatus = @delegate.Invoke(handle, ref names, ref descr);
                if (driversStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the drivers", mpg123_errors.MPG123_ERR);
                driverCount = driversStatus;
                driverNames = ArrayVariantLength.GetStringsKnownLength(names, driverCount);
                driverDescs = ArrayVariantLength.GetStringsKnownLength(descr, driverCount);
            }

            // Iterate through each driver, but ignore the builtins as they're used for debugging.
            for (int i = 0; i < driverCount; i++)
            {
                string name = driverNames[i];
                string description = driverDescs[i];
                if (description.Contains("(builtin)"))
                    continue;
                drivers.Add(name, description);
            }
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            Dictionary<string, string> devices = [];

            // We're now entering the dangerous zone
            nint names = IntPtr.Zero, descr = IntPtr.Zero, active = IntPtr.Zero;
            int deviceCount;
            string[] deviceNames, deviceDescs;
            unsafe
            {
                // Query the devices
                var handle = basolia._out123Handle;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_devices>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_devices));
                int devicesStatus = @delegate.Invoke(handle, driver, out names, out descr, ref active);
                if (devicesStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the devices", mpg123_errors.MPG123_ERR);
                activeDevice = Marshal.PtrToStringAnsi(active);
                deviceCount = devicesStatus;
                deviceNames = ArrayVariantLength.GetStringsKnownLength(names, deviceCount);
                deviceDescs = ArrayVariantLength.GetStringsKnownLength(descr, deviceCount);
            }

            // Iterate through each device
            for (int i = 0; i < deviceCount; i++)
            {
                string name = deviceNames[i];
                string description = deviceDescs[i];
                devices.Add(name, description);
            }
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);

            // We're now entering the dangerous zone
            unsafe
            {
                // Query the devices
                var handle = basolia._out123Handle;
                IntPtr driverPtr = IntPtr.Zero;
                IntPtr devicePtr = IntPtr.Zero;
                var @delegate = MpgNative.GetDelegate<NativeOutputLib.out123_driver_info>(MpgNative.libManagerOut, nameof(NativeOutputLib.out123_driver_info));
                int devicesStatus = @delegate.Invoke(handle, ref driverPtr, ref devicePtr);
                if (devicesStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the devices", mpg123_errors.MPG123_ERR);
                string driver = Marshal.PtrToStringAnsi(driverPtr);
                string device = Marshal.PtrToStringAnsi(devicePtr);
                return (driver, device);
            }
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            var driverList = GetDrivers(basolia);
            if (!driverList.ContainsKey(driver))
                throw new BasoliaException($"Driver {driver} doesn't exist", mpg123_errors.MPG123_ERR);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            basolia.activeDevice = "";
            var deviceList = GetDevices(basolia, driver, ref basolia.activeDevice);
            if (string.IsNullOrEmpty(device))
                return;
            if (!deviceList.ContainsKey(device))
                throw new BasoliaException($"Device {device} doesn't exist", mpg123_errors.MPG123_ERR);
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
                throw new BasoliaException("Basolia instance is not provided", mpg123_errors.MPG123_BAD_HANDLE);
            basolia.activeDriver = null;
            basolia.activeDevice = null;
        }
    }
}
