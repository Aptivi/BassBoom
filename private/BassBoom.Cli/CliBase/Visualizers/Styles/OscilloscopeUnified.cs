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
using System.Text;
using BassBoom.Basolia.Media.Format;
using Terminaux.Base;
using Terminaux.Base.Structures;
using Terminaux.Writer.CyclicWriters.Renderer;
using Terminaux.Writer.CyclicWriters.Simple;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class OscilloscopeUnified : IVisualizer
    {
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the samples and downsample them
            int width = ConsoleWrapper.WindowWidth;
            float[] cachedStereoLeft = new float[Visualizer.sample.left.Length];
            float[] cachedStereoRight = new float[Visualizer.sample.right.Length];
            Visualizer.sample.left.CopyTo(cachedStereoLeft, 0);
            Visualizer.sample.right.CopyTo(cachedStereoRight, 0);
            var downsampledLeft = FormatTools.DownsampleSamples(cachedStereoLeft, width);
            var downsampledRight = FormatTools.DownsampleSamples(cachedStereoRight, width);

            // Get the left oscilloscope and render it
            int height = ConsoleWrapper.WindowHeight;
            int quarterHeight = height / 4;
            int halfHeight = height / 2;
            for (int currentY = 0; currentY < quarterHeight; currentY++)
            {
                var progress = Oscilloscope.GetProgressFrom(downsampledLeft, false);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            for (int currentY = quarterHeight; currentY < halfHeight; currentY++)
            {
                var progress = Oscilloscope.GetProgressFrom(downsampledLeft, true);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            for (int currentY = halfHeight; currentY < halfHeight + quarterHeight; currentY++)
            {
                var progress = Oscilloscope.GetProgressFrom(downsampledRight, false);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            for (int currentY = halfHeight + quarterHeight; currentY < height; currentY++)
            {
                var progress = Oscilloscope.GetProgressFrom(downsampledRight, true);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        { }
    }
}
