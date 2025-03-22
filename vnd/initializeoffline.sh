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

# Restore packages
echo "- Restoring packages..."
echo "  - HOME=$ROOTDIR/nuget dotnet restore $ROOTDIR/BassBoom.sln"
HOME=$ROOTDIR/nuget dotnet restore $ROOTDIR/BassBoom.sln
checkerror $? "  - Failed to restore NuGet packages"

# Download libmpv for Windows
echo "- Downloading libmpv for Windows..."
echo "  - curl -L --output $ROOTDIR/vnd/mpv-dev-x86_64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-x86_64-20250225-git-5459b0f.7z"
curl -L --output $ROOTDIR/vnd/mpv-dev-x86_64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-x86_64-20250225-git-5459b0f.7z
echo "  - curl -L --output $ROOTDIR/vnd/mpv-dev-aarch64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-aarch64-20250225-git-5459b0f.7z"
curl -L --output $ROOTDIR/vnd/mpv-dev-aarch64-20250225-git-5459b0f.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-02-25-5459b0f/mpv-dev-aarch64-20250225-git-5459b0f.7z

# Copy dependencies to deps
echo "- Copying dependencies to deps..."
echo "  - mkdir $ROOTDIR/deps"
mkdir $ROOTDIR/deps
checkerror $? "  - Failed to mkdir deps"
echo "  - cp $ROOTDIR/nuget/.nuget/packages/*/*/*.nupkg $ROOTDIR/deps/"
cp $ROOTDIR/nuget/.nuget/packages/*/*/*.nupkg $ROOTDIR/deps/
checkerror $? "  - Failed to copy deps"
echo "  - rm -rf $ROOTDIR/nuget"
rm -rf $ROOTDIR/nuget
checkerror $? "  - Failed to remove nuget folder"

# Copy NuGet.config for offline use
echo "- Copying NuGet.config..."
echo "  - cp $ROOTDIR/vnd/OfflineNuGet.config $ROOTDIR/NuGet.config"
cp $ROOTDIR/vnd/OfflineNuGet.config $ROOTDIR/NuGet.config
checkerror $? "  - Failed to copy offline NuGet config"

echo "- You should be able to build offline!"

