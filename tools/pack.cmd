@echo off

set ROOTDIR=%~dp0\..

REM Run any vendor actions on pack
if exist %ROOTDIR%\vnd\vendor-pack.cmd call %ROOTDIR%\vnd\vendor-pack.cmd %*
if %errorlevel% neq 0 goto :failure

REM Inform success
echo Pack successful
goto :finished

:failure
echo Pack failed

:finished
