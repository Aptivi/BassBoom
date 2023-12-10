//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
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

namespace BassBoom.Native.Runtime
{
    /// <summary>
    /// The uname types
    /// </summary>
    public enum UnameTypes
    {
        /// <summary>
        /// The kernel name
        /// </summary>
        KernelName = 1,
        /// <summary>
        /// The network node host name (usually a hostname)
        /// </summary>
        NetworkNode = 2,
        /// <summary>
        /// Kernel release version
        /// </summary>
        KernelRelease = 4,
        /// <summary>
        /// Kernel release extended version
        /// </summary>
        KernelVersion = 8,
        /// <summary>
        /// Machine type
        /// </summary>
        Machine = 16,
        /// <summary>
        /// Operating system type
        /// </summary>
        OperatingSystem = 32,
        /// <summary>
        /// All! same as "uname -a"
        /// </summary>
        All = KernelName | NetworkNode | KernelRelease | KernelVersion | Machine | OperatingSystem
    }
}
