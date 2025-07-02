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

using BassBoom.Basolia;
using BassBoom.Cli.CliBase;
using System;
using System.IO;
using System.Reflection;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Base.Extensions;
using System.Linq;
using Terminaux.Colors;
using Terminaux.Colors.Data;
using Terminaux.Base;
using BassBoom.Basolia.Playback;
using BassBoom.Cli.Languages;

namespace BassBoom.Cli
{
    internal class BassBoomCli
    {
        private static readonly Version? version = Assembly.GetAssembly(typeof(InitBasolia))?.GetName().Version;
        internal static Version? mpgVer;
        internal static Version? outVer;
        internal static BasoliaMedia? basolia;
        internal static Color white = new(ConsoleColors.White);

        static int Main(string[] args)
        {
            try
            {
                ConsoleMisc.SetTitle($"BassBoom CLI - Basolia v{version?.ToString(3)} - Beta {version?.Minor}");

                // First, prompt for the music path if no arguments are provided.
                string[] arguments = args.Where((arg) => !arg.StartsWith("-")).ToArray();
                string[] switches = args.Where((arg) => arg.StartsWith("-")).ToArray();
                bool isRadio = switches.Contains("-r");
                if (arguments.Length != 0)
                {
                    string musicPath = args[0];

                    // Check for existence.
                    if (string.IsNullOrEmpty(musicPath) || (!isRadio && !File.Exists(musicPath)))
                    {
                        TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_APP_NOTFOUND"), musicPath);
                        return 1;
                    }
                    if (!isRadio)
                        Player.passedMusicPaths.Add(musicPath);
                }

                // Initialize Basolia
                basolia = new();

                // Initialize versions
                mpgVer = InitBasolia.MpgLibVersion;
                outVer = InitBasolia.OutLibVersion;

                // Now, open an interactive TUI
                ConsoleResizeHandler.StartResizeListener((_, _, _, _) => Common.redraw = true);
                if (isRadio)
                    Radio.RadioLoop();
                else
                    Player.PlayerLoop();

                // Close the output if necessary
                PlaybackTools.CloseOutput(basolia);
            }
            catch (Exception ex)
            {
                TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_APP_FATALERROR") + "\n\n" + ex.ToString());
                return ex.HResult;
            }
            return 0;
        }
    }
}
