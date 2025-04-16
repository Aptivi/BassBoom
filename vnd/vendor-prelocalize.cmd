@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250416-git-4697f7c.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-16-4697f7c/mpv-dev-x86_64-20250416-git-4697f7c.7z -OutFile %TEMP%\mpv-dev-x86_64-20250416-git-4697f7c.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250416-git-4697f7c.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-16-4697f7c/mpv-dev-aarch64-20250416-git-4697f7c.7z -OutFile %TEMP%\mpv-dev-aarch64-20250416-git-4697f7c.7z"
