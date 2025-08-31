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
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Writer.FancyWriters;
using Terminaux.Inputs;
using System.Collections.Generic;
using Terminaux.Inputs.Styles.Selection;
using BassBoom.Basolia.Exceptions;
using Terminaux.Inputs.Styles;
using Terminaux.Writer.CyclicWriters.Renderer.Tools;
using Terminaux.Writer.MiscWriters;
using Terminaux.Writer.CyclicWriters;
using BassBoom.Cli.Languages;

namespace BassBoom.Cli.CliBase
{
    internal static class Equalizer
    {
        internal static bool exiting = false;
        internal static int currentBandIdx = 0;

        internal static Keybinding[] ShowBindings =>
        [
            new(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_KEYBINDING_DECREASE"), ConsoleKey.LeftArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_KEYBINDING_INCREASE"), ConsoleKey.RightArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_KEYBINDING_PREVBAND"), ConsoleKey.UpArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_KEYBINDING_NEXTBAND"), ConsoleKey.DownArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_KEYBINDING_RESET"), ConsoleKey.R),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_QUIT"), ConsoleKey.Q),
        ];

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
                    var keystroke = Input.ReadKey();
                    HandleKeypress(keystroke);
                }
                catch (BasoliaException bex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_BASOLIAERROR") + "\n\n" + bex.Message);
                }
                catch (BasoliaOutException bex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_BASOLIAOUTERROR") + "\n\n" + bex.Message);
                }
                catch (Exception ex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_ERROR") + "\n\n" + ex.Message);
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
                    Common.redraw = true;
                    break;
            }
        }

        private static string HandleDraw()
        {
            // Prepare things
            var drawn = new StringBuilder();
            ConsoleWrapper.CursorVisible = false;

            // First, print the keystrokes
            var keybindings = new Keybindings()
            {
                KeybindingList = ShowBindings,
                Left = 0,
                Top = ConsoleWrapper.WindowHeight - 1,
                Width = ConsoleWrapper.WindowWidth - 1,
            };
            drawn.Append(keybindings.Render());

            // Write current song
            string name = LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_NOTPLAYING");
            if (Common.cachedInfos.Count > 0)
                name = Common.isRadioMode ? RadioControls.RenderStationName() : PlayerControls.RenderSongName(Common.CurrentCachedInfo?.MusicPath ?? "");

            // Now, print the list of bands and their values.
            var choices = new List<InputChoiceInfo>();
            int startPos = 4;
            int endPos = ConsoleWrapper.WindowHeight - 1;
            int bandsPerPage = endPos - startPos;
            for (int i = 0; i < 32; i++)
            {
                // Get the equalizer value for this band
                double val = EqualizerControls.GetEqualizer(i);
                string eqType =
                    i == 0 ? LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_BASS") :
                    i == 1 ? LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_UPPERMID") :
                    i == 2 ? LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_TREBLE") :
                    i > 2 ? LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_DEVICEBAND") :
                    LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_UNKNOWNBAND");

                // Now, render it
                string bandData = $"[{val:0.00}] " + LanguageTools.GetLocalized("BASSBOOM_APP_EQUALIZER_BAND") + $" #{i + 1} - {eqType}";
                choices.Add(new($"{i + 1}", bandData));
            }

            // Print the list of bands and their values.
            var bandBoxFrame = new BoxFrame()
            {
                Text = name,
                Left = 2,
                Top = 1,
                InteriorWidth = ConsoleWrapper.WindowWidth - 6,
                InteriorHeight = bandsPerPage
            };
            var bandSelections = new Selection([.. choices])
            {
                Left = 3,
                Top = 2,
                CurrentSelection = currentBandIdx,
                Height = bandsPerPage,
                Width = ConsoleWrapper.WindowWidth - 6,
                Settings = new()
                {
                    SelectedOptionColor = ConsoleColors.Green,
                    OptionColor = ConsoleColors.Silver,
                }
            };
            drawn.Append(
                bandBoxFrame.Render() +
                bandSelections.Render()
            );
            return drawn.ToString();
        }
    }
}
