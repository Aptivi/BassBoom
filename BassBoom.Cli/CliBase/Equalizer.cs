﻿//
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
using BassBoom.Basolia;
using System;
using System.Text;
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;
using Terminaux.Reader;
using Terminaux.Inputs;
using System.Collections.Generic;
using Terminaux.Inputs.Styles.Selection;

namespace BassBoom.Cli.CliBase
{
    internal static class Equalizer
    {
        internal static bool exiting = false;
        internal static int currentBandIdx = 0;

        internal static void OpenEqualizer(Screen screen)
        {
            // First, initialize a screen part to handle drawing
            ScreenPart screenPart = new();
            screenPart.AddDynamicText(HandleDraw);
            screen.RemoveBufferedParts();
            screen.AddBufferedPart("BassBoom Player - Equalizer", screenPart);

            // Then, clear the screen to draw our TUI
            while (!exiting)
            {
                try
                {
                    // Render the buffer
                    ScreenTools.Render();

                    // Handle the keystroke
                    var keystroke = TermReader.ReadKey();
                    HandleKeypress(keystroke);
                }
                catch (BasoliaException bex)
                {
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia when trying to process the equalizer operation.\n\n" + bex.Message);
                }
                catch (BasoliaOutException bex)
                {
                    InfoBoxColor.WriteInfoBox("There's an error with Basolia output when trying to process the equalizer operation.\n\n" + bex.Message);
                }
                catch (Exception ex)
                {
                    InfoBoxColor.WriteInfoBox("There's an unknown error when trying to process the equalizer operation.\n\n" + ex.Message);
                }
            }

            // Restore state
            exiting = false;
            screen.RemoveBufferedParts();
            ColorTools.LoadBack();
        }

        private static void HandleKeypress(ConsoleKeyInfo keystroke)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.RightArrow:
                    {
                        double eq = EqualizerControls.GetCachedEqualizer(currentBandIdx);
                        eq += 0.05d;
                        EqualizerControls.SetEqualizer(currentBandIdx, eq);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    {
                        double eq = EqualizerControls.GetCachedEqualizer(currentBandIdx);
                        eq -= 0.05d;
                        EqualizerControls.SetEqualizer(currentBandIdx, eq);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    currentBandIdx--;
                    if (currentBandIdx < 0)
                        currentBandIdx = 0;
                    break;
                case ConsoleKey.DownArrow:
                    currentBandIdx++;
                    if (currentBandIdx > 31)
                        currentBandIdx = 31;
                    break;
                case ConsoleKey.R:
                    EqualizerControls.ResetEqualizers();
                    break;
                case ConsoleKey.Q:
                    exiting = true;
                    break;
            }
        }

        private static string HandleDraw()
        {
            // Prepare things
            var drawn = new StringBuilder();
            ConsoleWrapper.CursorVisible = false;
            ColorTools.LoadBack();

            // First, print the keystrokes
            string keystrokes =
                "[<-|->] Change" +
                " - [UP|DOWN] Select Band" +
                " - [R] Reset" +
                " - [Q] Exit";
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 2, keystrokes));

            // Print the separator and the music file info
            string separator = new('═', ConsoleWrapper.WindowWidth);
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 4, separator));

            // Write powered by...
            drawn.Append(TextWriterWhereColor.RenderWhere($"╣ Powered by BassBoom and MPG123 v{BassBoomCli.mpgVer} ╠", 2, ConsoleWrapper.WindowHeight - 4));

            // Write current song
            if (Player.cachedInfos.Count > 0)
                drawn.Append(PlayerControls.RenderSongName(Player.cachedInfos[Player.currentSong - 1].MusicPath));
            else if (Radio.cachedInfos.Count > 0)
                drawn.Append(RadioControls.RenderStationName());

            // Now, print the list of bands and their values.
            var choices = new List<InputChoiceInfo>();
            int startPos = 3;
            int endPos = ConsoleWrapper.WindowHeight - 5;
            int bandsPerPage = endPos - startPos;
            for (int i = 0; i < 32; i++)
            {
                // Get the equalizer value for this band
                double val = EqualizerControls.GetEqualizer(i);
                string eqType =
                    // Bass bands: 1-8, Bass-Mid bands: 9-16, Mid-Treble bands: 17-24, Treble bands: 25-32
                    i < 4 ? "Deep Bass" : // Band 1, 2, 3, 4
                    i < 8 ? "Bass" :
                    i < 12 ? "Deep Bass-Mid" :
                    i < 16 ? "Bass-Mid" :
                    i < 20 ? "Deep Mid-Treble" :
                    i < 24 ? "Mid-Treble" :
                    i < 28 ? "Deep Treble" :
                    i < 32 ? "Treble" :
                    "Unknown band type";

                // Now, render it
                string bandData = $"[{val:0.00}] Equalizer Band #{i + 1} - {eqType}";
                choices.Add(new($"{i + 1}", bandData));
            }
            drawn.Append(
                SelectionInputTools.RenderSelections([.. choices], 2, 3, currentBandIdx, bandsPerPage, ConsoleWrapper.WindowWidth - 4, selectedForegroundColor: new Color(ConsoleColors.Green), foregroundColor: new Color(ConsoleColors.Silver))
            );
            return drawn.ToString();
        }
    }
}
