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
using System.IO;
using System.Text;
using System.Threading;
using BassBoom.Basolia.Media;
using BassBoom.QuickPlay.Arguments;
using BassBoom.QuickPlay.Languages;
using Colorimetry.Data;
using Colorimetry.Transformation;
using Terminaux.Base;
using Terminaux.Base.Extensions;
using Terminaux.Inputs;
using Terminaux.Shell.Arguments.Base;
using Terminaux.Themes.Colors;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.CyclicWriters.Simple;

namespace BassBoom.QuickPlay
{
    internal class QuickPlayer
    {
        internal static bool quiet;
        internal static string musicPath = "";
        private static Thread playerThread = new((basolia) => ((BasoliaMedia?)basolia)?.Play());
        private static readonly Dictionary<string, ArgumentInfo> arguments = new()
        {
            { "quiet", new("quiet", "Quiet mode", new QuietArgument()) },
            { "path", new("path", "Path to MPEG music file", new PathArgument()) },
        };

        static int Main(string[] args)
        {
            // Parse the arguments
            ArgumentParse.ParseArguments(args, arguments);

            // Check to see if a music file has been specified and is found
            if (string.IsNullOrEmpty(musicPath))
            {
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_NOTSPECIFIED"), ThemeColorType.Error);
                return 1;
            }
            if (!File.Exists(musicPath))
            {
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_NOTFOUND"), ThemeColorType.Error, musicPath);
                return 2;
            }
            if (!quiet)
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_OPENING"), ThemeColorType.Progress, musicPath);

            // Open the music file
            var basoliaMedia = new BasoliaMedia();
            basoliaMedia.OpenFile(musicPath);

            // Get metadata info and display them
            var (rate, channels, encoding) = basoliaMedia.GetFormatInfo();
            basoliaMedia.GetId3Metadata(out var metadatav1, out var metadatav2);
            string musicName =
                (!string.IsNullOrEmpty(metadatav2?.Title) ? metadatav2?.Title :
                 !string.IsNullOrEmpty(metadatav1?.Title) ? metadatav1?.Title :
                 Path.GetFileNameWithoutExtension(musicPath)) ?? "";
            string musicArtist =
                (!string.IsNullOrEmpty(metadatav2?.Artist) ? metadatav2?.Artist :
                 !string.IsNullOrEmpty(metadatav1?.Artist) ? metadatav1?.Artist :
                 LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_UNKNOWNARTIST")) ?? "";
            if (!quiet)
            {
                TextWriterColor.Write($" >> {musicArtist} - {musicName}", ThemeColorType.Success);
                TextWriterColor.Write($" >> {rate}, {channels}, {encoding}");
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_EXITTIP") + "\n", ThemeColorType.Tip);
            }

            // Play the music file, and display live status
            try
            {
                int duration = basoliaMedia.GetDuration(true);
                var durationProgress = new SimpleProgress(0, duration)
                {
                    ShowPercentage = false,
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
                        // Get duration information
                        int currentDuration = basoliaMedia.GetCurrentDuration();
                        string durationSpan = basoliaMedia.GetDurationSpanFromSamples(duration).ToString();
                        string currentDurationSpan = basoliaMedia.GetDurationSpanFromSamples(currentDuration).ToString();
                        string durationIndicator = $"{currentDurationSpan} / {durationSpan}";

                        // Get progress width
                        int durationIndicatorTextWidth = ConsoleChar.EstimateCellWidth(durationIndicator);
                        int totalWidth = ConsoleWrapper.WindowWidth - durationIndicatorTextWidth - 1;
                        durationProgress.Width = totalWidth;
                        durationProgress.Position = currentDuration;

                        // Display duration and progress
                        builder.Clear();
                        builder.Append('\r');
                        builder.Append($"{durationIndicator} {durationProgress.Render()}");
                        builder.Append(ConsoleClearing.GetClearLineToRightSequence());
                        TextWriterRaw.WriteRaw(builder.ToString());
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
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_QUICKPLAYER_FAILURE") + $": {ex.Message}", ThemeColorType.Error);
            }

            // Return success to the OS
            return 0;
        }
    }
}
