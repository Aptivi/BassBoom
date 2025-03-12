@echo off

set ROOTDIR=%~dp0\..

REM Run any vendor actions on push
if exist %ROOTDIR%\vnd\vendor-push.cmd call %ROOTDIR%\vnd\vendor-push.cmd %*
if %errorlevel% neq 0 goto :failure

REM Inform success
echo Push successful
goto :finished

:failure
echo Push failed

:finished
