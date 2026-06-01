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
using Terminaux.Base;
using Terminaux.Base.Structures;
using Terminaux.Writer.CyclicWriters.Renderer;
using Terminaux.Writer.CyclicWriters.Simple;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class Reflect : IVisualizer
    {
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the number of progress bars required
            float[] cachedBands = new float[32];
            Visualizer.bands.CopyTo(cachedBands, 0);
            float step = (float)cachedBands.Length / ConsoleWrapper.WindowWidth;
            int height = ConsoleWrapper.WindowHeight / 2;
            Debug.WriteLine(string.Join(", ", cachedBands));
            int posX = 0;
            int posY = height;
            for (float stepped = 0; stepped < cachedBands.Length; stepped += step)
            {
                // Get the band index and band value
                int bandIdx = (int)stepped;
                float band = cachedBands[bandIdx];

                // Describe it using progress bar
                int bandValue = (int)(band * 15);
                var progress = new SimpleProgress(bandValue, 100)
                {
                    Accurate = true,
                    Vertical = true,
                    ShowPercentage = false,
                    Height = height,
                };
                var reflectedProgress = new SimpleProgress(100 - bandValue, 100)
                {
                    Accurate = true,
                    Vertical = true,
                    ShowPercentage = false,
                    Height = height,
                    ProgressForegroundColor = progress.ProgressActiveForegroundColor,
                    ProgressActiveForegroundColor = progress.ProgressForegroundColor,
                };
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(posX, 0)));
                drawn.Append(RendererTools.RenderRenderable(reflectedProgress, new Coordinate(posX, posY)));

                // Increment X positions
                posX++;
            }
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        { }
    }
}
