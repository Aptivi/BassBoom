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
using Colorimetry;
using Colorimetry.Data;
using Colorimetry.Transformation;
using Terminaux.Base;
using Terminaux.Writer.CyclicWriters.Graphical;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class BandPads : IVisualizer
    {
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the cached bands
            float[] cachedBands = new float[32];
            Visualizer.bands.CopyTo(cachedBands, 0);

            // Get the blend level for the bands
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

            // Now, show the blends for bass, mid, and treble
            Color bassBeatColor = TransformationTools.BlendColor(ConsoleColors.Red, ConsoleColors.Black, BasoliaMedia.Clamp(bassBlendLevel, 0, 1000) / 10);
            Color midBeatColor = TransformationTools.BlendColor(ConsoleColors.Lime, ConsoleColors.Black, BasoliaMedia.Clamp(midBlendLevel, 0, 1000) / 10);
            Color trebleBeatColor = TransformationTools.BlendColor(ConsoleColors.Blue, ConsoleColors.Black, BasoliaMedia.Clamp(trebleBlendLevel, 0, 1000) / 10);

            // Determine how to state the blends
            int boxHeight = ConsoleWrapper.WindowHeight;
            int boxWidth = ConsoleWrapper.WindowWidth / 3;
            Box bassBeatBox = new()
            {
                Color = bassBeatColor,
                Height = boxHeight,
                Width = boxWidth,
                Left = 0,
                Top = 0,
            };
            Box midBeatBox = new()
            {
                Color = midBeatColor,
                Height = boxHeight,
                Width = boxWidth,
                Left = boxWidth,
                Top = 0,
            };
            Box trebleBeatBox = new()
            {
                Color = trebleBeatColor,
                Height = boxHeight,
                Width = boxWidth,
                Left = boxWidth * 2,
                Top = 0,
            };
            drawn.Append(
                bassBeatBox.Render() +
                midBeatBox.Render() +
                trebleBeatBox.Render()
            );
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        { }
    }
}
