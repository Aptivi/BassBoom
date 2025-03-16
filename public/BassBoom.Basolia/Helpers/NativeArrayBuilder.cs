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
using System.Runtime.InteropServices;
using System.Text;

namespace BassBoom.Basolia.Helpers
{
    internal static class NativeArrayBuilder
    {
        internal static IntPtr InitByteArrayPointers(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1;
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int i = 0; i < arr.Length; i++)
            {
                IntPtr unmanagedPointer = GetUtf8BytesPointer(arr[i]);
                byteArrayPointers[i] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }

        internal static byte[] GetUtf8Bytes(string s) =>
            Encoding.UTF8.GetBytes(s + "\0");

        internal static IntPtr GetUtf8BytesPointer(string s)
        {
            var bytes = GetUtf8Bytes(s);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            return unmanagedPointer;
        }
    }
}
