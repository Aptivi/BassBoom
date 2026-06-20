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
    internal class Oscilloscope : IVisualizer
    {
        int mode = 0;

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
            int halfHeight = ConsoleWrapper.WindowHeight / 2;
            for (int currentY = 0; currentY < halfHeight; currentY++)
            {
                var progress = GetProgressFrom(downsampledLeft, mode == 1);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            for (int currentY = halfHeight; currentY < ConsoleWrapper.WindowHeight; currentY++)
            {
                var progress = GetProgressFrom(downsampledRight, mode == 1);
                drawn.Append(RendererTools.RenderRenderable(progress, new Coordinate(0, currentY)));
            }
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        {
            mode++;
            if (mode > 1)
                mode = 0;
        }

        internal static SimpleProgress GetProgressFrom(float[] downsampled, bool useRms)
        {
            float peak = GetPeak(downsampled, useRms);

            // Render it to a simple progress bar renderer
            int bar = (int)(peak * 100);
            var progress = new SimpleProgress(bar, 100)
            {
                Accurate = true,
                ShowPercentage = false,
                Height = ConsoleWrapper.WindowHeight,
                Width = ConsoleWrapper.WindowWidth,
            };
            return progress;
        }

        internal static float GetPeak(float[] downsampled, bool useRms)
        {
            // Decide whether to get the peak loudness or the RMS loudness
            float peak = 0;
            if (useRms)
                peak = GetRmsLoudness(downsampled);
            else
            {
                foreach (float d in downsampled)
                    if (d > peak)
                        peak = d;
            }
            return peak;
        }

        private static float GetRmsLoudness(float[] downsampled)
        {
            float rms = 0;
            foreach (float d in downsampled)
                rms += d * d;
            return (float)Math.Sqrt(rms / downsampled.Length);
        }
    }
}
