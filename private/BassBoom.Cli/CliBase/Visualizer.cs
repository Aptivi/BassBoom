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
using System.Threading;
using BassBoom.Basolia.Exceptions;
using BassBoom.Cli.CliBase.Visualizers;
using BassBoom.Cli.CliBase.Visualizers.Styles;
using BassBoom.Cli.Languages;
using Terminaux.Base;
using Terminaux.Base.Buffered;
using Terminaux.Base.Extensions;
using Terminaux.Inputs;
using Terminaux.Inputs.Styles.Infobox;
using Terminaux.Inputs.Styles.Infobox.Tools;
using Terminaux.Writer.CyclicWriters.Renderer.Tools;

namespace BassBoom.Cli.CliBase
{
    internal static class Visualizer
    {
        internal static bool exiting = false;
        internal static float[] bands = new float[32];
        internal static (float[] left, float[] right) sample = default;
        internal static int currentVisualizer = 0;
        internal static IVisualizer[] visualizers =
        [
            new Bars(),
            new Beats(),
            new BandPads(),
            new Circles(),
            new Oscilloscope(),
            new Reflect(),
        ];

        internal static Keybinding[] ShowBindings =>
        [
            new(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_KEYBINDING_PREVIOUS"), ConsoleKey.LeftArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_KEYBINDING_NEXT"), ConsoleKey.RightArrow),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_KEYBINDING_MODE"), ConsoleKey.M),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_QUIT"), ConsoleKey.Q),
            new(LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_KEYBINDING_HELP"), ConsoleKey.H),
        ];

        internal static IVisualizer CurrentVisualizer =>
            visualizers[currentVisualizer];

        internal static void OpenVisualizer(Screen screen)
        {
            // First, initialize a screen part to handle drawing
            ScreenPart screenPart = new();
            screenPart.AddDynamicText(HandleDraw);
            screen.RemoveBufferedParts();
            screen.AddBufferedPart("BassBoom Player - Equalizer", screenPart);
            ConsoleColoring.AllowBackground = true;

            // Then, clear the screen to draw our TUI
            while (!exiting)
            {
                try
                {
                    // Render the buffer
                    ScreenTools.Render();

                    // Obtain input
                    Thread.Sleep(20);
                    InputEventInfo? keystroke = Input.ReadPointerOrKeyNoBlock();

                    // Handle the keystroke
                    if (keystroke.ConsoleKeyInfo is ConsoleKeyInfo cki && !Input.PointerActive)
                    {
                        HandleKeypress(cki, screen);
                        screen.RequireRefresh();
                    }
                }
                catch (BasoliaException bex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_BASOLIAERROR") + "\n\n" + bex.Message);
                }
                catch (BasoliaOutException bex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_BASOLIAOUTERROR") + "\n\n" + bex.Message);
                }
                catch (Exception ex)
                {
                    InfoBoxModalColor.WriteInfoBoxModal(LanguageTools.GetLocalized("BASSBOOM_APP_VISUALIZER_ERROR") + "\n\n" + ex.Message);
                }
            }

            // Restore state
            ConsoleColoring.AllowBackground = false;
            exiting = false;
            screen.RemoveBufferedParts();
            ConsoleColoring.LoadBack();
        }

        private static void HandleKeypress(ConsoleKeyInfo keystroke, Screen screen)
        {
            switch (keystroke.Key)
            {
                case ConsoleKey.H:
                    InfoBoxModalColor.WriteInfoBoxModal(KeybindingTools.RenderKeybindingHelpText(ShowBindings), new InfoBoxSettings()
                    {
                        Title = LanguageTools.GetLocalized("BASSBOOM_APP_COMMON_AVAILABLEKEYSTROKES"),
                    });
                    screen.RequireRefresh();
                    break;
                case ConsoleKey.LeftArrow:
                    currentVisualizer--;
                    if (currentVisualizer < 0)
                        currentVisualizer = visualizers.Length - 1;
                    break;
                case ConsoleKey.RightArrow:
                    currentVisualizer++;
                    if (currentVisualizer > visualizers.Length - 1)
                        currentVisualizer = 0;
                    break;
                case ConsoleKey.UpArrow:
                    Common.RaiseVolume();
                    break;
                case ConsoleKey.DownArrow:
                    Common.LowerVolume();
                    break;
                case ConsoleKey.M:
                    CurrentVisualizer.SwitchMode();
                    break;
                case ConsoleKey.Q:
                    exiting = true;
                    ScreenTools.CurrentScreen?.RequireRefresh();
                    break;
            }
        }

        private static string HandleDraw()
        {
            // Draw the visualizer
            ConsoleWrapper.CursorVisible = false;
            return CurrentVisualizer.DrawVisualizer();
        }
    }
}
