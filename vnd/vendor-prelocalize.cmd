@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250428-git-f8cef99.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-28-f8cef99/mpv-dev-x86_64-20250428-git-f8cef99.7z -OutFile %TEMP%\mpv-dev-x86_64-20250428-git-f8cef99.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250428-git-f8cef99.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-28-f8cef99/mpv-dev-aarch64-20250428-git-f8cef99.7z -OutFile %TEMP%\mpv-dev-aarch64-20250428-git-f8cef99.7z"
