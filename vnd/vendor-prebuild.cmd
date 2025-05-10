@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250510-git-b3070d1.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-10-b3070d1/mpv-dev-x86_64-20250510-git-b3070d1.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250510-git-b3070d1.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250510-git-b3070d1.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-10-b3070d1/mpv-dev-aarch64-20250510-git-b3070d1.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250510-git-b3070d1.7z"""

pushd "%ROOTDIR%\tools\"
"%ProgramFiles%\7-Zip\7z.exe" x "%TEMP%\mpv-dev-x86_64-20250510-git-b3070d1.7z" libmpv-2.dll
popd
mkdir "%ROOTDIR%\public\BassBoom.Native\runtimes\win-x64\native\"
move "%ROOTDIR%\tools\libmpv-2.dll" "%ROOTDIR%\public\BassBoom.Native\runtimes\win-x64\native\"

pushd "%ROOTDIR%\tools\"
"%ProgramFiles%\7-Zip\7z.exe" x "%TEMP%\mpv-dev-aarch64-20250510-git-b3070d1.7z" libmpv-2.dll
popd
mkdir "%ROOTDIR%\public\BassBoom.Native\runtimes\win-arm64\native\"
move "%ROOTDIR%\tools\libmpv-2.dll" "%ROOTDIR%\public\BassBoom.Native\runtimes\win-arm64\native\"
