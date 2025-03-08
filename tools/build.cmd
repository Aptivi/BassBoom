@echo off

REM This script builds and packs the artifacts. Use when you have VS installed.
set releaseconfig=%1
if "%releaseconfig%" == "" set releaseconfig=Release

set buildoptions=%*
call set buildoptions=%%buildoptions:*%1=%%
if "%buildoptions%" == "*=" set buildoptions=

REM Turn off telemetry and logo
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_NOLOGO=1

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250225-git-5459b0f.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-x86_64-20250225-git-5459b0f.7z -OutFile %TEMP%\mpv-dev-x86_64-20250225-git-5459b0f.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250225-git-5459b0f.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-aarch64-20250225-git-5459b0f.7z -OutFile %TEMP%\mpv-dev-aarch64-20250225-git-5459b0f.7z"

pushd %ROOTDIR%\tools\
"%ProgramFiles%\7-Zip\7z.exe" x %TEMP%\mpv-dev-x86_64-20250225-git-5459b0f.7z libmpv-2.dll
popd
mkdir %ROOTDIR%\public\BassBoom.Native\runtimes\win-x64\native\
move %ROOTDIR%\tools\libmpv-2.dll %ROOTDIR%\public\BassBoom.Native\runtimes\win-x64\native\

pushd %ROOTDIR%\tools\
"%ProgramFiles%\7-Zip\7z.exe" x %TEMP%\mpv-dev-aarch64-20250225-git-5459b0f.7z libmpv-2.dll
popd
mkdir %ROOTDIR%\public\BassBoom.Native\runtimes\win-arm64\native\
move %ROOTDIR%\tools\libmpv-2.dll %ROOTDIR%\public\BassBoom.Native\runtimes\win-arm64\native\

:download
echo Downloading packages...
"%ProgramFiles%\dotnet\dotnet.exe" restore "%ROOTDIR%\BassBoom.sln" -p:Configuration=%releaseconfig% %buildoptions%
if %errorlevel% == 0 goto :build
echo There was an error trying to download packages (%errorlevel%).
goto :finished

:build
echo Building BassBoom...
"%ProgramFiles%\dotnet\dotnet.exe" build "%ROOTDIR%\BassBoom.sln" -p:Configuration=%releaseconfig% %buildoptions%
if %errorlevel% == 0 goto :success
echo There was an error trying to build (%errorlevel%).
goto :finished

:success
echo Build successful.
:finished
