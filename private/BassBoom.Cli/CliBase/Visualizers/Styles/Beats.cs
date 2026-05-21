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

using System.Diagnostics;
using System.Text;
using BassBoom.Basolia.Media;
using Colorimetry;
using Colorimetry.Data;
using Colorimetry.Transformation;
using Terminaux.Base.Extensions;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class Beats : IVisualizer
    {
        int mode = 0;
        
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the cached bands
            float[] cachedBands = new float[32];
            Visualizer.bands.CopyTo(cachedBands, 0);
            Debug.WriteLine(string.Join(", ", cachedBands));

            // Get the blend level for the bands
            int startBand = mode * 10;
            int endBand = (mode + 1) * 10 - 1;
            int blendLevel = 0;
            for (int bandIdx = startBand; bandIdx <= endBand; bandIdx++)
            {
                // Get the band value
                float band = cachedBands[bandIdx];
                int bandValue = (int)(band * 20);
                blendLevel += bandValue;
            }

            // Determine how to state the blend
            Color beatColor = mode == 0 ? ConsoleColors.Red : mode == 1 ? ConsoleColors.Lime : ConsoleColors.Blue;
            Color finalBeatColor = TransformationTools.BlendColor(beatColor, ConsoleColors.Black, BasoliaMedia.Clamp(blendLevel, 0, 1000) / 10);
            drawn.Append(
                ConsoleColoring.RenderSetConsoleColor(finalBeatColor, true) +
                ConsoleClearing.GetClearWholeScreenSequence() +
                ConsoleColoring.RenderResetBackground()
            );
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        {
            mode++;
            if (mode > 2)
                mode = 0;
        }
    }
}
