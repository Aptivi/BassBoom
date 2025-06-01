@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250601-git-730062b.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-01-730062b/mpv-dev-x86_64-20250601-git-730062b.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250601-git-730062b.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250601-git-730062b.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-01-730062b/mpv-dev-aarch64-20250601-git-730062b.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250601-git-730062b.7z"""
