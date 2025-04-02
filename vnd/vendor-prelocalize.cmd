@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250328-git-5ba7ee5.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-03-28-5ba7ee5/mpv-dev-x86_64-20250328-git-5ba7ee5.7z -OutFile %TEMP%\mpv-dev-x86_64-20250328-git-5ba7ee5.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250328-git-5ba7ee5.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-03-28-5ba7ee5/mpv-dev-aarch64-20250328-git-5ba7ee5.7z -OutFile %TEMP%\mpv-dev-aarch64-20250328-git-5ba7ee5.7z"
