#!/bin/bash

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Convenience functions
checkerror() {
    if [ $1 != 0 ]
    then
        printf "$2 - Error $1\n" >&2
        exit $1
    fi
}

# This script builds.
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi

# Check for dependencies
dotnetpath=`which dotnet`
checkerror $? "dotnet is not found"
sevenzpath=`which 7z`
checkerror $? "7z is not found"

# Turn off telemetry and logo
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

# Download compiled Windows libmpv libraries
if [ ! -f /tmp/mpv-dev-x86_64-20250225-git-5459b0f.7z ]; then
    curl -L --output /tmp/mpv-dev-x86_64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-x86_64-20250225-git-5459b0f.7z
fi
if [ ! -f /tmp/mpv-dev-aarch64-20250225-git-5459b0f.7z ]; then
    curl -L --output /tmp/mpv-dev-aarch64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-aarch64-20250225-git-5459b0f.7z
fi
cd $ROOTDIR/tools && "$sevenzpath" x /tmp/mpv-dev-x86_64-20250225-git-5459b0f.7z libmpv-2.dll && cd -
mkdir -p $ROOTDIR/public/BassBoom.Native/runtimes/win-x64/native/
mv $ROOTDIR/tools/libmpv-2.dll $ROOTDIR/public/BassBoom.Native/runtimes/win-x64/native/

cd $ROOTDIR/tools && "$sevenzpath" x /tmp/mpv-dev-aarch64-20250225-git-5459b0f.7z libmpv-2.dll && cd -
mkdir -p $ROOTDIR/public/BassBoom.Native/runtimes/win-arm64/native/
mv $ROOTDIR/tools/libmpv-2.dll $ROOTDIR/public/BassBoom.Native/runtimes/win-arm64/native/

# Download packages
echo Downloading packages...
"$dotnetpath" restore "$ROOTDIR/BassBoom.sln" -p:Configuration=$releaseconf ${@:2}
checkerror $? "Failed to download packages"

# Build
echo Building...
"$dotnetpath" build "$ROOTDIR/BassBoom.sln" -p:Configuration=$releaseconf ${@:2}
checkerror $? "Failed to build"

# Inform success
echo Build successful.
