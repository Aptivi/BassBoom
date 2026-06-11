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

using System.Text;
using BassBoom.Basolia.Media;
using Colorimetry.Data;
using Terminaux.Base;
using Terminaux.Base.Extensions;
using Terminaux.Writer.CyclicWriters.Graphical.Shapes;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class Circles : IVisualizer
    {
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the number of circles required
            float[] cachedBands = new float[32];
            Visualizer.bands.CopyTo(cachedBands, 0);

            // Get the height and the width required
            int height = ConsoleWrapper.WindowHeight;
            int width = ConsoleWrapper.WindowWidth / 2;
            var bassCircle = new Arc(height, 0, 0, ConsoleColors.Red)
            {
                Width = width
            };
            var midCircle = new Arc(height, 0, 0, ConsoleColors.Lime)
            {
                Width = width
            };
            var trebleCircle = new Arc(height, 0, 0, ConsoleColors.Blue)
            {
                Width = width
            };

            // Get the circle size, depending on the band values
            int startBassBand = 0;
            int endBassBand = startBassBand + 10 - 1;
            int bassBlendLevel = 0;
            for (int bandIdx = startBassBand; bandIdx <= endBassBand; bandIdx++)
            {
                // Get the band value
                float band = cachedBands[bandIdx];
                int bandValue = (int)(band * 20);
                bassBlendLevel += bandValue;
            }
            bassBlendLevel = BasoliaMedia.Clamp(bassBlendLevel, 0, 1000);
            bassCircle.OuterRadius = bassBlendLevel / height;

            // Mids...
            int startMidBand = endBassBand + 1;
            int endMidBand = startMidBand + 10 - 1;
            int midBlendLevel = 0;
            for (int bandIdx = startMidBand; bandIdx <= endMidBand; bandIdx++)
            {
                // Get the band value
                float band = cachedBands[bandIdx];
                int bandValue = (int)(band * 20);
                midBlendLevel += bandValue;
            }
            midBlendLevel = BasoliaMedia.Clamp(midBlendLevel, 0, 1000);
            midCircle.OuterRadius = midBlendLevel / height;

            // Treble...
            int startTrebleBand = endMidBand + 1;
            int endTrebleBand = startTrebleBand + 10 - 1;
            int trebleBlendLevel = 0;
            for (int bandIdx = startTrebleBand; bandIdx <= endTrebleBand; bandIdx++)
            {
                // Get the band value
                float band = cachedBands[bandIdx];
                int bandValue = (int)(band * 20);
                trebleBlendLevel += bandValue;
            }
            trebleBlendLevel = BasoliaMedia.Clamp(trebleBlendLevel, 0, 1000);
            trebleCircle.OuterRadius = trebleBlendLevel / height;

            // Draw the circles
            drawn.Append(
                ConsoleClearing.GetClearWholeScreenSequence() +
                ConsoleColoring.RenderResetBackground() +
                bassCircle.Render() +
                midCircle.Render() +
                trebleCircle.Render()
            );
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        { }
    }
}
