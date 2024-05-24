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

using BassBoom.Native.Interop.Analysis;
using BassBoom.Native.Interop.Synthesis;
using System;
using System.Collections.Generic;

namespace BassBoom.Basolia.Synthesis
{
    /// <summary>
    /// Synthesis tools for MPG123
    /// </summary>
    public static class SynthesisTools
    {
        private static readonly Dictionary<Guid, SynthesizerInfo> synths = [];

        public static void NewSynthesis(string synthName, out Guid synthId)
        {
            unsafe
            {
                int error = 0;
                var synth = NativeSynthesis.syn123_new(44100, 2, (int)mpg123_enc_enum.MPG123_ENC_ANY, 0, ref error);
                if (error == 0)
                {
                    synthId = Guid.NewGuid();
                    synths.Add(synthId, new SynthesizerInfo(synth, synthName));
                }
                else
                    throw new BasoliaSynException($"Can't initialize synthesizer for synth {synthName}", (syn123_error)error);
            }
        }

        public static bool DoesSynthesizerExist(Guid synthId) =>
            synths.ContainsKey(synthId);

        public static void SynthSweep(Guid synthId, syn123_wave_id waveId, double phase, int backwards, syn123_sweep_id sweepId, double f1, double f2, int smooth, IntPtr duration, out double endphase, out IntPtr period, out IntPtr bufferPeriod)
        {
            if (!DoesSynthesizerExist(synthId))
                throw new BasoliaSynException($"Synthesizer {synthId} not found.", syn123_error.SYN123_BAD_HANDLE);

            unsafe
            {
                var synHandle = synths[synthId].synHandle;
                endphase = 0;
                int intPeriod = 0;
                int intBufferPeriod = 0;
                int result = NativeSynthesis.syn123_setup_sweep(synHandle, waveId, phase, backwards, sweepId, f1, f2, smooth, duration.ToInt32(), ref endphase, ref intPeriod, ref intBufferPeriod);
                if (result < 0)
                    throw new BasoliaSynException("Failed to generate frequency sweep.", (syn123_error)result);
                period = (IntPtr)intPeriod;
                bufferPeriod = (IntPtr)intBufferPeriod;
            }
        }
    }
}
