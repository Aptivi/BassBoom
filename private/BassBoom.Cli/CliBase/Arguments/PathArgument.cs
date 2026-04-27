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
using System.IO;
using BassBoom.Cli.Languages;
using Terminaux.Shell.Arguments.Base;
using Terminaux.Writer.ConsoleWriters;

namespace BassBoom.Cli.CliBase.Arguments
{
    internal class PathArgument : ArgumentExecutor, IArgument
    {
        public override void Execute(ArgumentParameters parameters)
        {
            foreach (var musicPath in parameters.ArgumentsList)
            {
                // Check for existence.
                if (string.IsNullOrEmpty(musicPath) || (!BassBoomCli.isRadio && !File.Exists(musicPath)))
                {
                    TextWriterColor.Write(LanguageTools.GetLocalized("BASSBOOM_APP_NOTFOUND"), musicPath);
                    Environment.Exit(1);
                }
                if (!BassBoomCli.isRadio)
                    Player.passedMusicPaths.Add(musicPath);
                else
                    Radio.passedRadioStationPaths.Add(musicPath);
            }
        }
    }
}
