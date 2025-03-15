@echo off

REM    BassBoom  Copyright (C) 2023-2025  Aptivi
REM
REM    This file is part of BassBoom
REM
REM    BassBoom is free software: you can redistribute it and/or modify
REM    it under the terms of the GNU General Public License as published by
REM    the Free Software Foundation, either version 3 of the License, or
REM    (at your option) any later version.
REM
REM    BassBoom is distributed in the hope that it will be useful,
REM    but WITHOUT ANY WARRANTY; without even the implied warranty of
REM    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
REM    GNU General Public License for more details.
REM
REM    You should have received a copy of the GNU General Public License
REM    along with this program.  If not, see <https://www.gnu.org/licenses/>.

REM This script runs BassBoom. This is a shortcut for running BassBoom
REM so that you don't have to write the full name of the executable.

set ROOTPATH=%~dp0
dotnet "%ROOTPATH%BassBoom.Cli.dll" %*
