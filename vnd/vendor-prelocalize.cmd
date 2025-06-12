@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250612-git-47bc7b2.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-12-47bc7b2/mpv-dev-x86_64-20250612-git-47bc7b2.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250612-git-47bc7b2.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250612-git-47bc7b2.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-12-47bc7b2/mpv-dev-aarch64-20250612-git-47bc7b2.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250612-git-47bc7b2.7z"""
