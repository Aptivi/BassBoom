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
using BassBoom.Basolia.Media.Format;
using Colorimetry;
using Colorimetry.Data;
using Colorimetry.Transformation;
using Terminaux.Base;
using Terminaux.Base.Extensions;
using Terminaux.Writer.CyclicWriters.Graphical;

namespace BassBoom.Cli.CliBase.Visualizers.Styles
{
    internal class LoudnessBack : IVisualizer
    {
        int mode = 0;
        
        string IVisualizer.DrawVisualizer()
        {
            var drawn = new StringBuilder();

            // Get the samples and downsample them
            float[] cachedStereoLeft = new float[Visualizer.sample.left.Length];
            float[] cachedStereoRight = new float[Visualizer.sample.right.Length];
            Visualizer.sample.left.CopyTo(cachedStereoLeft, 0);
            Visualizer.sample.right.CopyTo(cachedStereoRight, 0);
            var downsampledLeft = FormatTools.DownsampleSamples(cachedStereoLeft, 100);
            var downsampledRight = FormatTools.DownsampleSamples(cachedStereoRight, 100);

            // Get the peak level, normalize it to 1000, then blend the color
            float peakLeft = Oscilloscope.GetPeak(downsampledLeft, mode == 1);
            float peakRight = Oscilloscope.GetPeak(downsampledRight, mode == 1);
            Color loudnessColorLeft = TransformationTools.BlendColor(ConsoleColors.Lime, ConsoleColors.Black, BasoliaMedia.Clamp((int)(peakLeft * 1000), 0, 1000) / 10);
            Color loudnessColorRight = TransformationTools.BlendColor(ConsoleColors.Lime, ConsoleColors.Black, BasoliaMedia.Clamp((int)(peakRight * 1000), 0, 1000) / 10);

            // Determine how to state the loudness
            int boxHeight = ConsoleWrapper.WindowHeight;
            int boxWidth = ConsoleWrapper.WindowWidth / 2;
            Box leftChannelBox = new()
            {
                Color = loudnessColorLeft,
                Height = boxHeight,
                Width = boxWidth,
                Left = 0,
                Top = 0,
            };
            Box rightChannelBox = new()
            {
                Color = loudnessColorRight,
                Height = boxHeight,
                Width = boxWidth,
                Left = boxWidth,
                Top = 0,
            };
            drawn.Append(
                leftChannelBox.Render() +
                rightChannelBox.Render()
            );
            return drawn.ToString();
        }

        void IVisualizer.SwitchMode()
        {
            mode++;
            if (mode > 1)
                mode = 0;
        }
    }
}
