@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250610-git-59d5f77.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-10-59d5f77/mpv-dev-x86_64-20250610-git-59d5f77.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250610-git-59d5f77.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250610-git-59d5f77.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-10-59d5f77/mpv-dev-aarch64-20250610-git-59d5f77.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250610-git-59d5f77.7z"""
