@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist %TEMP%\mpv-dev-x86_64-20250427-git-b47c805.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-27-b47c805/mpv-dev-x86_64-20250427-git-b47c805.7z -OutFile %TEMP%\mpv-dev-x86_64-20250427-git-b47c805.7z"
if not exist %TEMP%\mpv-dev-aarch64-20250427-git-b47c805.7z powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-04-27-b47c805/mpv-dev-aarch64-20250427-git-b47c805.7z -OutFile %TEMP%\mpv-dev-aarch64-20250427-git-b47c805.7z"
