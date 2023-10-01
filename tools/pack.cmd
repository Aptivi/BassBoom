@echo off

REM    BassBoom  Copyright (C) 2023  Aptivi
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

for /f "tokens=* USEBACKQ" %%f in (`type version`) do set version=%%f
set releaseconfig=%1
if "%releaseconfig%" == "" set releaseconfig=Release

:packbin
echo Packing binary...
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-bin.rar "..\BassBoom\bin\%releaseconfig%\net7.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-api.rar "..\BassBoom.Basolia\bin\%releaseconfig%\net7.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-proto.rar "..\BassBoom.Desktop\bin\%releaseconfig%\net7.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-protocli.rar "..\BassBoom.Cli\bin\%releaseconfig%\net7.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-unsafeapi.rar "..\BassBoom.Native\bin\%releaseconfig%\net7.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-bin-win.rar "..\BassBoom\bin\%releaseconfig%\net7.0-windows\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-api-win.rar "..\BassBoom.Basolia\bin\%releaseconfig%\net7.0-windows\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-proto-win.rar "..\BassBoom.Desktop\bin\%releaseconfig%\net7.0-windows\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-protocli-win.rar "..\BassBoom.Cli\bin\%releaseconfig%\net7.0-windows\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-unsafeapi-win.rar "..\BassBoom.Native\bin\%releaseconfig%\net7.0-windows\"
if %errorlevel% == 0 goto :complete
echo There was an error trying to pack binary (%errorlevel%).
goto :finished

:complete
move %temp%\%version%-bin.rar
move %temp%\%version%-api.rar
move %temp%\%version%-proto.rar
move %temp%\%version%-protocli.rar
move %temp%\%version%-unsafeapi.rar
move %temp%\%version%-bin-win.rar
move %temp%\%version%-api-win.rar
move %temp%\%version%-proto-win.rar
move %temp%\%version%-protocli-win.rar
move %temp%\%version%-unsafeapi-win.rar

echo Pack successful.
:finished
