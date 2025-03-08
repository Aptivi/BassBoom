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

# This script builds and packs the artifacts. Use when you have MSBuild installed.
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi
version=$(grep "<Version>" $ROOTDIR/Directory.Build.props | cut -d "<" -f 2 | cut -d ">" -f 2)
checkerror $? "Failed to get version. Check to make sure that the version is specified correctly in D.B.props"

# Check for dependencies
zippath=`which zip`
checkerror $? "zip is not found"

# Pack binary
echo Packing binary...
cd "$ROOTDIR/private/BassBoom.Cli/bin/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-cli.zip . && cd -
checkerror $? "Failed to pack"
cd "$ROOTDIR/private/BassBoom.Cli/bin/$releaseconf/net48/" && "$zippath" -r /tmp/$version-cli-48.zip . && cd -
checkerror $? "Failed to pack"

# Inform success
mv /tmp/$version-cli.zip $ROOTDIR/tools/
checkerror $? "Failed to move archive from temporary folder"
mv /tmp/$version-cli-48.zip $ROOTDIR/tools/
checkerror $? "Failed to move archive from temporary folder"
echo Build and pack successful.
exit 0
