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
        [StructLayout(LayoutKind.Sequential)]
        internal struct ElementList
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] elements;
        }

        internal static string[] GetStringsKnownLength(IntPtr arrayPointer, int elements)
        {
            var stringsEnum = Marshal.PtrToStructure<ElementList>(arrayPointer);
            List<string> strings = [];
            for (int i = 0; i < elements; i++)
            {
                var element = stringsEnum.elements[i];
                string value = Marshal.PtrToStringAnsi(element);
                strings.Add(value);
            }
            return [.. strings];
        }

        internal static string[] GetStringsUnknownLength(IntPtr arrayPointer)
        {
            var stringsEnum = Marshal.PtrToStructure<ElementList>(arrayPointer);
            List<string> strings = [];
            foreach (var element in stringsEnum.elements)
            {
                if (element == IntPtr.Zero)
                    break;
                strings.Add(Marshal.PtrToStringAnsi(element));
            }
            return [.. strings];
        }
    }
}
