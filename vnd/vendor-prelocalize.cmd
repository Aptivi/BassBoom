@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250417-git-0dc24a2.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-17-0dc24a2/mpv-dev-x86_64-20250417-git-0dc24a2.7z -OutFile %TEMP%\mpv-dev-x86_64-20250417-git-0dc24a2.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250417-git-0dc24a2.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-17-0dc24a2/mpv-dev-aarch64-20250417-git-0dc24a2.7z -OutFile %TEMP%\mpv-dev-aarch64-20250417-git-0dc24a2.7z"
