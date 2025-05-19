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
using BassBoom.Native;
using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Enumerations;
using BassBoom.Native.Interop.Init;
using System;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Helpers
{
    /// <summary>
    /// MPV property handler
    /// </summary>
    public static class MpvPropertyHandler
    {
        /// <summary>
        /// Sets an MPV string property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="propertyValue">Property value to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetStringProperty(BasoliaMedia? basolia, string propertyName, string propertyValue)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Set the string property
                var handle = basolia._libmpvHandle;
                var propertyValuePointer = NativeArrayBuilder.GetUtf8BytesPointer(propertyValue);
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_set_property>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_set_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_STRING, ref propertyValue);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to set string property {propertyName} to {propertyValue}", propertyResult);
            }
        }
        
        /// <summary>
        /// Sets an MPV integer property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="propertyValue">Property value to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetIntegerProperty(BasoliaMedia? basolia, string propertyName, long propertyValue)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Set the string property
                var handle = basolia._libmpvHandle;
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_set_property_int>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_set_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_INT64, ref propertyValue);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to set string property {propertyName} to {propertyValue}", propertyResult);
            }
        }
        
        /// <summary>
        /// Sets an MPV double property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="propertyValue">Property value to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static void SetDoubleProperty(BasoliaMedia? basolia, string propertyName, double propertyValue)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            unsafe
            {
                // Set the string property
                var handle = basolia._libmpvHandle;
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_set_property_double>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_set_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_DOUBLE, ref propertyValue);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to set string property {propertyName} to {propertyValue}", propertyResult);
            }
        }

        /// <summary>
        /// Gets an MPV string property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static string GetStringProperty(BasoliaMedia? basolia, string propertyName)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            string value = "";
            unsafe
            {
                // Get the string property
                var handle = basolia._libmpvHandle;
                var buffer = IntPtr.Zero;
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_get_property>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_get_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_STRING, out buffer);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to get string property {propertyName}", propertyResult);

                // Convert the integer pointer to the string
                value = Marshal.PtrToStringAnsi(buffer);
            }

            // Return the property value
            return value;
        }

        /// <summary>
        /// Gets an MPV integer number property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static long GetIntegerProperty(BasoliaMedia? basolia, string propertyName)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            long value = 0;
            unsafe
            {
                // Get the string property
                var handle = basolia._libmpvHandle;
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_get_property_int>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_get_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_INT64, out value);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to get string property {propertyName}", propertyResult);
            }

            // Return the property value
            return value;
        }

        /// <summary>
        /// Gets an MPV double number property
        /// </summary>
        /// <param name="basolia">Basolia instance that contains a valid handle</param>
        /// <param name="propertyName">Property name to set</param>
        /// <exception cref="BasoliaException"></exception>
        public static double GetDoubleProperty(BasoliaMedia? basolia, string propertyName)
        {
            InitBasolia.CheckInited();
            if (basolia is null)
                throw new BasoliaException("Basolia instance is not provided", MpvError.MPV_ERROR_INVALID_PARAMETER);

            // We're now entering the dangerous zone
            double value = 0;
            unsafe
            {
                // Get the string property
                var handle = basolia._libmpvHandle;
                MpvError propertyResult = (MpvError)NativeInitializer.GetDelegate<NativeParameters.mpv_get_property_double>(NativeInitializer.libManagerMpv, nameof(NativeParameters.mpv_get_property)).Invoke(handle, propertyName, MpvValueFormat.MPV_FORMAT_INT64, out value);
                if (propertyResult < MpvError.MPV_ERROR_SUCCESS)
                    throw new BasoliaException($"Failed to get string property {propertyName}", propertyResult);
            }

            // Return the property value
            return value;
        }
    }
}
