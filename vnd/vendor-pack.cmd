@echo off

set ROOTDIR=%~dp0\..

for /f "tokens=*" %%g in ('findstr "<Version>" "%ROOTDIR%\Directory.Build.props"') do (set MIDVER=%%g)
for /f "tokens=1 delims=<" %%a in ("%MIDVER:~9%") do (set version=%%a)
set releaseconfig=%1
if "%releaseconfig%" == "" set releaseconfig=Release

:packbin
echo Packing binary...
"%ProgramFiles%\7-Zip\7z.exe" a -tzip "%temp%/%version%-cli.zip" "%ROOTDIR%\private\BassBoom.Cli\bin\%releaseconfig%\net10.0\*"
"%ProgramFiles%\7-Zip\7z.exe" a -tzip "%temp%/%version%-cli-48.zip" "%ROOTDIR%\private\BassBoom.Cli\bin\%releaseconfig%\net48\*"
if %errorlevel% == 0 goto :complete
echo There was an error trying to pack binary (%errorlevel%).
goto :finished

:complete
move "%temp%\%version%-cli.zip" "%ROOTDIR%\tools\"
move "%temp%\%version%-cli-48.zip" "%ROOTDIR%\tools\"

:finished
