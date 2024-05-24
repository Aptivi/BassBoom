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

namespace BassBoom.Native.Interop.Synthesis
{
    /// <summary>
    /// Synthesizer info
    /// </summary>
    public unsafe class SynthesizerInfo
    {
        internal syn123_handle* synHandle;
        private string synName;

        /// <summary>
        /// Synthesizer name
        /// </summary>
        public string SynthesizerName =>
            synName;

        internal SynthesizerInfo(syn123_handle* synHandle, string synName)
        {
            this.synHandle = synHandle;
            this.synName = synName;
        }
    }
}
