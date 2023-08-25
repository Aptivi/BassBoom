
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Runtime;
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
        internal static string activeDriver = "";
        internal static string activeDevice = "";

        [StructLayout(LayoutKind.Sequential)]
        public struct DriverList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] listOfDrivers;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DriverDescList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] listOfDriverDescriptions;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] listOfDevices;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceDescList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] listOfDeviceDescriptions;
        }

        public static ReadOnlyDictionary<string, string> GetDrivers()
        {
            InitBasolia.CheckInited();
            Dictionary<string, string> drivers = new();

            // We're now entering the dangerous zone
            nint names = nint.Zero, descr = nint.Zero;
            int driverCount;
            DriverList drvList;
            DriverDescList drvDescList;
            unsafe
            {
                // Query the drivers
                var handle = Mpg123Instance._out123Handle;
                int driversStatus = NativeOutputLib.out123_drivers(handle, ref names, ref descr);
                if (driversStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the drivers", mpg123_errors.MPG123_ERR);
                drvList = Marshal.PtrToStructure<DriverList>(names);
                drvDescList = Marshal.PtrToStructure<DriverDescList>(descr);
                driverCount = driversStatus;
            }

            // Iterate through each driver
            for (int i = 0; i < driverCount; i++)
            {
                string name = Marshal.PtrToStringAnsi(drvList.listOfDrivers[i]);
                string description = Marshal.PtrToStringAnsi(drvDescList.listOfDriverDescriptions[i]);
                drivers.Add(name, description);
            }
            return new ReadOnlyDictionary<string, string>(drivers);
        }

        public static ReadOnlyDictionary<string, string> GetDevices(string driver, ref string activeDevice)
        {
            InitBasolia.CheckInited();
            Dictionary<string, string> devices = new();

            // We're now entering the dangerous zone
            nint names = nint.Zero, descr = nint.Zero, active = nint.Zero;
            int deviceCount;
            DeviceList devList;
            DeviceDescList devDescList;
            unsafe
            {
                // Query the devices
                var handle = Mpg123Instance._out123Handle;
                int devicesStatus = NativeOutputLib.out123_devices(handle, driver, ref names, ref descr, ref active);
                if (devicesStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the devices", mpg123_errors.MPG123_ERR);
                devList = Marshal.PtrToStructure<DeviceList>(names);
                devDescList = Marshal.PtrToStructure<DeviceDescList>(descr);
                activeDevice = Marshal.PtrToStringAnsi(active);
                deviceCount = devicesStatus;
            }

            // Iterate through each device
            for (int i = 0; i < deviceCount; i++)
            {
                string name = Marshal.PtrToStringAnsi(devList.listOfDevices[i]);
                string description = Marshal.PtrToStringAnsi(devDescList.listOfDeviceDescriptions[i]);
                devices.Add(name, description);
            }
            return new ReadOnlyDictionary<string, string>(devices);
        }

        public static void SetActiveDriver(string driver)
        {
            var driverList = GetDrivers();
            if (!driverList.ContainsKey(driver))
                throw new BasoliaException($"Driver {driver} doesn't exist", mpg123_errors.MPG123_ERR);
            activeDriver = driver;
        }

        public static void SetActiveDevice(string driver, string device)
        {
            var deviceList = GetDevices(driver, ref activeDevice);
            if (string.IsNullOrEmpty(device))
                return;
            if (!deviceList.ContainsKey(device))
                throw new BasoliaException($"Device {device} doesn't exist", mpg123_errors.MPG123_ERR);
            activeDevice = device;
        }
    }
}
