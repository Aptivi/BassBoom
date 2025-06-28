@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250628-git-ed8954e.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-x86_64-20250628-git-ed8954e.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250628-git-ed8954e.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250628-git-ed8954e.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-aarch64-20250628-git-ed8954e.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250628-git-ed8954e.7z"""
