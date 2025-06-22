@echo off

set ROOTDIR=%~dp0\..

REM Download libmpv for Windows and build
if not exist "%TEMP%\mpv-dev-x86_64-20250622-git-18defc8.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-22-18defc8/mpv-dev-x86_64-20250622-git-18defc8.7z -OutFile ""%TEMP%\mpv-dev-x86_64-20250622-git-18defc8.7z"""
if not exist "%TEMP%\mpv-dev-aarch64-20250622-git-18defc8.7z" powershell -Command "Invoke-WebRequest https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-22-18defc8/mpv-dev-aarch64-20250622-git-18defc8.7z -OutFile ""%TEMP%\mpv-dev-aarch64-20250622-git-18defc8.7z"""
