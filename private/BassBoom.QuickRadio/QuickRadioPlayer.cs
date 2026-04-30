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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BassBoom.Basolia.Media;
using BassBoom.QuickRadio.Arguments;
using BassBoom.QuickRadio.Languages;
using Colorimetry.Data;
using Colorimetry.Transformation;
using Terminaux.Base;
using Terminaux.Base.Extensions;
using Terminaux.Inputs;
using Terminaux.Shell.Arguments.Base;
using Terminaux.Themes.Colors;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.CyclicWriters.Simple;

namespace BassBoom.QuickRadio
{
    internal class QuickRadioPlayer
    {
        internal static bool quiet;
        internal static string radioPath = "";
        private static Thread playerThread = new((basolia) => ((BasoliaMedia?)basolia)?.Play());
        private static readonly Dictionary<string, ArgumentInfo> arguments = new()
        {
            { "quiet", new("quiet", "Quiet mode", new QuietArgument()) },
            { "path", new("path", "Path to MPEG radio URL", new PathArgument()) },
        };

        static int Main(string[] args)
        {
            // Parse the arguments
            ArgumentParse.ParseArguments(args, arguments);

            // Check to see if a music file has been specified and is found
            if (string.IsNullOrEmpty(radioPath))
            {
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKRADIO_NOTSPECIFIED"), ThemeColorType.Error);
                return 1;
            }
            if (!quiet)
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKRADIO_OPENING"), ThemeColorType.Progress, radioPath);

            // Open the music file
            var basoliaMedia = new BasoliaMedia();
            basoliaMedia.OpenUrl(radioPath);

            // Get current song
            if (!quiet)
            {
                TextWriterColor.Write($" >> {basoliaMedia.GetRadioNowPlaying()}\n", ThemeColorType.Success);
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKRADIO_EXITTIP") + "\n", ThemeColorType.Tip);
            }

            // Play the radio station, and display live status to indicate constant connection
            try
            {
                var durationProgress = new SimpleProgress(0, 0)
                {
                    ShowPercentage = false,
                    Indeterminate = true,
                    ProgressActiveForegroundColor = ConsoleColors.Lime,
                    ProgressForegroundColor = TransformationTools.GetDarkBackground(ConsoleColors.Lime),
                };
                var builder = new StringBuilder();
                playerThread.Start(basoliaMedia);
                SpinWait.SpinUntil(basoliaMedia.IsPlaying);
                while (basoliaMedia.IsPlaying())
                {
                    if (!quiet)
                    {
                        // Get progress width
                        int totalWidth = ConsoleWrapper.WindowWidth;
                        durationProgress.Width = totalWidth;

                        // Display duration and progress
                        builder.Clear();
                        builder.Append('\r');
                        builder.Append(durationProgress.Render());
                        builder.Append(ConsoleClearing.GetClearLineToRightSequence());
                        TextWriterRaw.WriteRaw(builder.ToString());
                        Thread.Sleep(20);
                    }

                    // Check to see if a user pressed a key
                    var keypress = Input.ReadPointerOrKeyNoBlock(InputEventType.Keyboard);
                    if (keypress.ConsoleKeyInfo is ConsoleKeyInfo cki && cki.Key == ConsoleKey.Q)
                        basoliaMedia.Stop();
                }
                if (!quiet)
                    TextWriterRaw.Write();
            }
            catch (Exception ex)
            {
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKRADIO_FAILURE") + $": {ex.Message}", ThemeColorType.Error);
            }

            // Return success to the OS
            return 0;
        }
    }
}
