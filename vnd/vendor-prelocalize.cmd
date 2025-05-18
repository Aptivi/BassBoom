@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250518-git-817372a.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-18-817372a/mpv-dev-x86_64-20250518-git-817372a.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250518-git-817372a.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250518-git-817372a.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-18-817372a/mpv-dev-aarch64-20250518-git-817372a.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250518-git-817372a.7z"""
