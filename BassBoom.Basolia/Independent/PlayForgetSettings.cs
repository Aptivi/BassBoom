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

namespace BassBoom.Basolia.Independent
{
    /// <summary>
    /// Settings for the Play and Forget technique
    /// </summary>
    public class PlayForgetSettings
    {
        private double volume = 1;
        private bool volBoost = false;
        private readonly string rootLibPath = "";

        /// <summary>
        /// Volume boost
        /// </summary>
        public bool VolumeBoost
        {
            get => volBoost;
            set
            {
                volBoost = value;
                if (!value)
                    volume = volume > 1 ? 1 : volume;
            }
        }

        /// <summary>
        /// Volume in fractional floating point integer (0.5 resembles 50%)
        /// </summary>
        public double Volume
        {
            get => volume;
            set
            {
                double maximum = VolumeBoost ? 3 : 1;
                volume = value < 0 ? 0 : value > maximum ? maximum : value;
            }
        }

        /// <summary>
        /// Root path to the library
        /// </summary>
        public string RootLibPath =>
            rootLibPath;

        /// <summary>
        /// Makes a new instance of the Play/Forget technique settings
        /// </summary>
        /// <exception cref="BasoliaMiscException"></exception>
        public PlayForgetSettings()
        { }

        /// <summary>
        /// Makes a new instance of the Play/Forget technique settings
        /// </summary>
        /// <param name="volume">Volume boost</param>
        /// <param name="volBoost">Volume in fractional floating point integer (0.5 resembles 50%)</param>
        /// <param name="rootLibPath">Root path to the library</param>
        /// <exception cref="BasoliaMiscException"></exception>
        public PlayForgetSettings(double volume, bool volBoost, string rootLibPath)
        {
            this.volume = volume;
            this.volBoost = volBoost;
            this.rootLibPath = rootLibPath ??
                throw new BasoliaMiscException("Provide a root library path.");
        }
    }
}
