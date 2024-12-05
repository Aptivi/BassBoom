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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BassBoom.Basolia.Helpers
{
    internal static class ArrayVariantLength
    {
        internal static string[] GetStringsKnownLength(IntPtr arrayPointer, int elements)
        {
            List<string> strings = [];
            for (int i = 0; i < elements; i++)
            {
                IntPtr elementPtr = Marshal.ReadIntPtr(arrayPointer, i * IntPtr.Size);
                string value = Marshal.PtrToStringAnsi(elementPtr);
                strings.Add(value);
            }
            return [.. strings];
        }

        internal static string[] GetStringsUnknownLength(IntPtr arrayPointer)
        {
            List<string> strings = [];
            for (int i = 0; i < int.MaxValue; i++)
            {
                IntPtr elementPtr = Marshal.ReadIntPtr(arrayPointer, i * IntPtr.Size);
                if (elementPtr == IntPtr.Zero)
                    break;
                string value = Marshal.PtrToStringAnsi(elementPtr);
                strings.Add(value);
            }
            return [.. strings];
        }

        internal static int[] GetIntegersKnownLength(IntPtr arrayPointer, int elements, int size)
        {
            List<int> ints = [];
            for (int i = 0; i < elements; i++)
            {
                IntPtr elementPtr = arrayPointer + i * size;
                int value = Marshal.ReadInt32(elementPtr);
                ints.Add(value);
            }
            return [.. ints];
        }
    }
}
