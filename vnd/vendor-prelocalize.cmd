@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250506-git-d702e5f.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-06-d702e5f/mpv-dev-x86_64-20250506-git-d702e5f.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250506-git-d702e5f.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250506-git-d702e5f.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-05-06-d702e5f/mpv-dev-aarch64-20250506-git-d702e5f.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250506-git-d702e5f.7z"""
