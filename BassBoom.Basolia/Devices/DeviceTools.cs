using BassBoom.Native.Interop.Init;
using BassBoom.Native.Interop.Output;
using BassBoom.Native.Runtime;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BassBoom.Basolia.Devices
{
    /// <summary>
    /// Device and driver tools
    /// </summary>
    public static class DeviceTools
    {
        public static ReadOnlyDictionary<string, string> GetDrivers()
        {
            InitBasolia.CheckInited();
            Dictionary<string, string> drivers = new();

            // We're now entering the dangerous zone
            string[] names, descr;
            unsafe
            {
                // Query the drivers
                var handle = Mpg123Instance._out123Handle;
                int driversStatus = NativeOutputLib.out123_drivers(handle, out names, out descr);
                if (driversStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the drivers", mpg123_errors.MPG123_ERR);
            }

            // Iterate through each driver
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                string description = descr[i];
                drivers.Add(name, description);
            }
            return new ReadOnlyDictionary<string, string>(drivers);
        }

        public static ReadOnlyDictionary<string, string> GetDevices(string driver)
        {
            InitBasolia.CheckInited();
            Dictionary<string, string> devices = new();

            // We're now entering the dangerous zone
            string[] names, descr;
            unsafe
            {
                // Query the devices
                var handle = Mpg123Instance._out123Handle;
                int devicesStatus = NativeOutputLib.out123_devices(handle, driver, out names, out descr, out string _);
                if (devicesStatus == (int)mpg123_errors.MPG123_ERR)
                    throw new BasoliaException("Can't query the devices", mpg123_errors.MPG123_ERR);
            }

            // Iterate through each device
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                string description = descr[i];
                devices.Add(name, description);
            }
            return new ReadOnlyDictionary<string, string>(devices);
        }
    }
}
