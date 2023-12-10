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

using BassBoom.Native.Runtime;
using System;

namespace BassBoom.Basolia
{
    /// <summary>
    /// Initialization code
    /// </summary>
    public static class InitBasolia
    {
        internal static bool _fugitive = false;
        private static bool _basoliaInited = false;
        
        /// <summary>
        /// Initializes the MPG123 library for Basolia to function
        /// </summary>
        /// <param name="root">Sets the root path to the library files</param>
        /// <param name="Fugitive">Allows illegal operations. NEVER SET THIS TO TRUE UNLESS YOU KNOW WHAT YOU'RE DOING!</param>
        public static void Init(string root = "", bool Fugitive = false)
        {
            if (string.IsNullOrEmpty(root))
                Mpg123Instance.InitializeLibrary();
            else
            {
                string mpg = Mpg123Instance.GetAppropriateMpg123LibraryPath(root);
                string @out = Mpg123Instance.GetAppropriateOut123LibraryPath(root);
                string syn = Mpg123Instance.GetAppropriateSyn123LibraryPath(root);
                Mpg123Instance.InitializeLibrary(mpg, @out, syn);
            }
            _fugitive = Fugitive;
            _basoliaInited = true;
        }

        public static void CheckInited()
        {
            if (!_basoliaInited)
                throw new InvalidOperationException("Basolia didn't initialize the MPG123 library yet!");
        }
    }
}
