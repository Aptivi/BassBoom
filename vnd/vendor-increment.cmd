@echo off

set oldVer=%1
set newVer=%2
set oldApiVer=%3
set newApiVer=%4
if "%oldVer%" == "" (
    echo Old version must be specified.
    exit /b 1
)
if "%newVer%" == "" (
    echo New version must be specified to replace old version "%oldVer%".
    exit /b 1
)
if "%oldApiVer%" == "" (
    echo Old API version must be specified.
    exit /b 1
)
if "%newApiVer%" == "" (
    echo New API version must be specified to replace old version "%oldApiVer%".
    exit /b 1
)

set ROOTDIR=%~dp0..

REM This script replaces old version with new version.
echo Replacing...

REM Change the below files
set "releaseFiles="%ROOTDIR%\PKGBUILD-REL" "%ROOTDIR%\.github\workflows\build-ppa-package-with-lintian.yml" "%ROOTDIR%\.github\workflows\build-ppa-package.yml" "%ROOTDIR%\.github\workflows\pushamend.yml" "%ROOTDIR%\.github\workflows\pushppa.yml" "%ROOTDIR%\.gitlab\workflows\release.yml" "%ROOTDIR%\public\BassBoom.Installers\BassBoom.Installer\Package.wxs" "%ROOTDIR%\public\BassBoom.Installers\BassBoom.InstallerBundle\Bundle.wxs" "%ROOTDIR%\Directory.Build.props" "%ROOTDIR%\CHANGES.TITLE""
for %%f in (%releaseFiles%) do (
    echo Processing %%f...
    powershell -Command "& '%ROOTDIR%\vnd\eng\incrementor.ps1' '%%f' '%oldVer%' '%newVer%' '%oldApiVer%' '%newApiVer%'"
)

REM Run the version changer
powershell -Command "& '%ROOTDIR%\vnd\eng\bb-version-change.ps1' '%ROOTDIR%' '%oldApiVer%' '%newApiVer%'"

REM Add Debian changelogs info
echo Changing Debian changelogs info
powershell -Command "& '%ROOTDIR%\vnd\eng\debian-changes.ps1' '%ROOTDIR%\debian\changelog' '%newVer%' '%newApiVer%'"
