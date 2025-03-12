@echo off

set ROOTDIR=%~dp0\..

REM Run any vendor actions on docgen pack
if exist %ROOTDIR%\vnd\vendor-docpack.cmd call %ROOTDIR%\vnd\vendor-docpack.cmd %*
if %errorlevel% neq 0 goto :failure

REM Inform success
echo Pack successful
goto :finished

:failure
echo Pack failed

:finished
