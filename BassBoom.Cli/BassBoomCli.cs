﻿
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using BassBoom.Cli.CliBase;
using System;
using System.IO;
using Terminaux.Reader.Inputs;
using Terminaux.Writer.ConsoleWriters;

namespace BassBoom.Cli
{
    internal class BassBoomCli
    {
        static int Main(string[] args)
        {
            try
            {
                // Essentials
                Console.Title = "BassBoom CLI - Basolia v0.0.1 - Pre-alpha";

                // First, prompt for the music path if no arguments are provided.
                string musicPath;
                if (args.Length == 0)
                {
                    TextWriterColor.Write("Provide a path to the music path in the below input:");
                    musicPath = Input.ReadLine();
                }
                else
                    musicPath = args[0];

                // Now, check for existence.
                if (string.IsNullOrEmpty(musicPath) || !File.Exists(musicPath))
                {
                    TextWriterColor.Write("Music file {0} doesn't exist.", musicPath);
                    return 1;
                }

                // Now, open an interactive TUI
                Player.PlayerLoop(musicPath);
            }
            catch (Exception ex)
            {
                TextWriterColor.Write("Fatal error in the BassBoom CLI.\n\n" + ex.ToString());
                return ex.HResult;
            }
            return 0;
        }
    }
}
