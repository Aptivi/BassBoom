//
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
using BassBoom.Basolia.Synthesis;
using BassBoom.Native.Interop.Synthesis;

namespace BassBoom.Synthesis.CliBase
{
    internal static class Synthesizer
    {
        internal static Version mpgVer;
        internal static Version outVer;
        internal static Version synVer;
        internal static bool exiting = false;
        internal static int currentSynthesizerIdx = 0;

        internal static void OpenSynthesizer()
        {
            // Initialize versions and synthesizers
            mpgVer = InitBasolia.MpgLibVersion;
            outVer = InitBasolia.OutLibVersion;
            synVer = InitBasolia.SynLibVersion;

            // Populate the screen
            Screen synthesizerScreen = new();
            ScreenTools.SetCurrent(synthesizerScreen);

            // Make a screen part to draw our TUI
            ScreenPart screenPart = new();

            // Handle drawing
            screenPart.AddDynamicText(HandleDraw);

            // Render the buffer
            synthesizerScreen.AddBufferedPart("BassBoom Synthesizer", screenPart);
            synthesizerScreen.ResetResize = false;

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
            ConsoleWrapper.CursorVisible = true;
            ColorTools.LoadBack();
            synthesizerScreen.RemoveBufferedParts();
            ScreenTools.UnsetCurrent(synthesizerScreen);
        }

        private static void HandleKeypress(ConsoleKeyInfo keystroke)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.RightArrow:
                    {
                        double eq = SynthesizerControls.GetCachedEqualizer(currentSynthesizerIdx);
                        eq += 0.05d;
                        SynthesizerControls.SetEqualizer(currentSynthesizerIdx, eq);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    {
                        double eq = SynthesizerControls.GetCachedEqualizer(currentSynthesizerIdx);
                        eq -= 0.05d;
                        SynthesizerControls.SetEqualizer(currentSynthesizerIdx, eq);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    currentSynthesizerIdx--;
                    if (currentSynthesizerIdx < 0)
                        currentSynthesizerIdx = 0;
                    break;
                case ConsoleKey.DownArrow:
                    currentSynthesizerIdx++;
                    if (currentSynthesizerIdx > 31)
                        currentSynthesizerIdx = 31;
                    break;
                case ConsoleKey.R:
                    SynthesizerControls.ResetEqualizers();
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
                " - [UP|DOWN] Select Synthesizer" +
                " - [SPACE] Play" +
                " - [Q] Exit";
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 2, keystrokes));

            // Print the separator and the music file info
            string separator = new('═', ConsoleWrapper.WindowWidth);
            drawn.Append(CenteredTextColor.RenderCentered(ConsoleWrapper.WindowHeight - 4, separator));

            // Write powered by...
            drawn.Append(TextWriterWhereColor.RenderWhere($"╣ Powered by BassBoom and MPG123 v{synVer} ╠", 2, ConsoleWrapper.WindowHeight - 4));

            // Now, print the list of synthesizers.
            var choices = new List<InputChoiceInfo>();
            int startPos = 3;
            int endPos = ConsoleWrapper.WindowHeight - 5;
            int synthsPerPage = endPos - startPos;
            for (int i = 0; i < 5; i++)
            {
                // Get the synth type
                string eqType =
                    i == 0 ? "Frequency Sweep" :
                    i == 1 ? "Pink Noise" :
                    i == 2 ? "White Noise" :
                    i == 3 ? "Geiger Sampling" :
                    i == 4 ? "Silence" :
                    "Unknown band type";

                // Now, render it
                string synthRendered = $"Synth #{i + 1} - {eqType}";
                choices.Add(new($"{i + 1}", synthRendered));
            }
            drawn.Append(
                SelectionInputTools.RenderSelections([.. choices], 2, 3, currentSynthesizerIdx, synthsPerPage, ConsoleWrapper.WindowWidth - 4, selectedForegroundColor: new Color(ConsoleColors.Green), foregroundColor: new Color(ConsoleColors.Silver))
            );
            return drawn.ToString();
        }

        private static void InitializeSynthesizers(out Guid sweepId, out Guid pinkNoiseId, out Guid whiteNoiseId, out Guid geigerId, out Guid silenceId)
        {
            SynthesisTools.NewSynthesis("Frequency Sweep", out sweepId);
            SynthesisTools.NewSynthesis("Pink Noise", out pinkNoiseId);
            SynthesisTools.NewSynthesis("White Noise", out whiteNoiseId);
            SynthesisTools.NewSynthesis("Geiger Sampling", out geigerId);
            SynthesisTools.NewSynthesis("Silence", out silenceId);

            // Set up a sample frequency sweep
            SynthesisTools.SynthSweep(sweepId, syn123_wave_id.SYN123_WAVE_SINE, 1, 0, syn123_sweep_id.SYN123_SWEEP_QUAD, 128, 512, 1, (IntPtr)1000, out _, out _, out _);
        }
    }
}
