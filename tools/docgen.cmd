@echo off

set ROOTDIR=%~dp0\..

REM Run any vendor actions on doc generation
if exist %ROOTDIR%\vnd\vendor-docgen.cmd call %ROOTDIR%\vnd\vendor-docgen.cmd %*
if %errorlevel% neq 0 goto :failure

REM Inform success
echo Build successful
goto :finished

:failure
echo Build failed

:finished
