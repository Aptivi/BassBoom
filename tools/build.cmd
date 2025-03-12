@echo off

set ROOTDIR=%~dp0\..

REM Run any vendor actions before build
if exist %ROOTDIR%\vnd\vendor-prebuild.cmd call %ROOTDIR%\vnd\vendor-prebuild.cmd %*
if %errorlevel% neq 0 goto :failure

REM Run any vendor actions on build
if exist %ROOTDIR%\vnd\vendor-build.cmd call %ROOTDIR%\vnd\vendor-build.cmd %*
if %errorlevel% neq 0 goto :failure

REM Run any vendor actions after build
if exist %ROOTDIR%\vnd\vendor-postbuild.cmd call %ROOTDIR%\vnd\vendor-postbuild.cmd %*
if %errorlevel% neq 0 goto :failure

REM Inform success
echo Build successful
goto :finished

:failure
echo Build failed

:finished
