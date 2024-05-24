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
using BassBoom.Synthesis.CliBase;
using System;
using System.Reflection;
using Terminaux.Base.Extensions;
using Terminaux.ResizeListener;
using Terminaux.Writer.ConsoleWriters;

internal class SynthesisCli
{
    private static readonly Version version = Assembly.GetAssembly(typeof(InitBasolia)).GetName().Version;

    static int Main()
    {
        try
        {
            ConsoleMisc.SetTitle($"BassBoom Synthesis CLI - Basolia v{version.ToString(3)} - Beta {version.Minor}");

            // Open an interactive TUI
            ConsoleResizeListener.StartResizeListener();
            InitBasolia.Init();
            Synthesizer.OpenSynthesizer();
        }
        catch (Exception ex)
        {
            TextWriterColor.Write("Fatal error in the BassBoom CLI.\n\n" + ex.ToString());
            return ex.HResult;
        }
        return 0;
    }
}
