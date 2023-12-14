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
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-bin.zip "..\BassBoom\bin\%releaseconfig%\net8.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-api.zip "..\BassBoom.Basolia\bin\%releaseconfig%\net8.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-proto.zip "..\BassBoom.Desktop\bin\%releaseconfig%\net8.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-protocli.zip "..\BassBoom.Cli\bin\%releaseconfig%\net8.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-unsafeapi.zip "..\BassBoom.Native\bin\%releaseconfig%\net8.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-bin-win.zip "..\BassBoom\bin\%releaseconfig%\net8.0-windows\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-api-win.zip "..\BassBoom.Basolia\bin\%releaseconfig%\net8.0-windows\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-proto-win.zip "..\BassBoom.Desktop\bin\%releaseconfig%\net8.0-windows\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-protocli-win.zip "..\BassBoom.Cli\bin\%releaseconfig%\net8.0-windows\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip %temp%/%version%-unsafeapi-win.zip "..\BassBoom.Native\bin\%releaseconfig%\net8.0-windows\*"
if %errorlevel% == 0 goto :complete
echo There was an error trying to pack binary (%errorlevel%).
goto :finished

:complete
move %temp%\%version%-bin.zip
move %temp%\%version%-api.zip
move %temp%\%version%-proto.zip
move %temp%\%version%-protocli.zip
move %temp%\%version%-unsafeapi.zip
move %temp%\%version%-bin-win.zip
move %temp%\%version%-api-win.zip
move %temp%\%version%-proto-win.zip
move %temp%\%version%-protocli-win.zip
move %temp%\%version%-unsafeapi-win.zip

echo Pack successful.
:finished
