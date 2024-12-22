#!/bin/bash

#   BassBoom  Copyright (C) 2023  Aptivi
# 
#   This file is part of BassBoom
# 
#   BassBoom is free software: you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#   the Free Software Foundation, either version 3 of the License, or
#   (at your option) any later version.
# 
#   BassBoom is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#   GNU General Public License for more details.
# 
#   You should have received a copy of the GNU General Public License
#   along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
version=$(grep "<Version>" ../Directory.Build.props | cut -d "<" -f 2 | cut -d ">" -f 2)
checkerror $? "Failed to get version. Check to make sure that the version is specified correctly in D.B.props"

# Check for dependencies
zippath=`which zip`
checkerror $? "zip is not found"

# Pack binary
echo Packing binary...
cd "../private/BassBoom.Cli/bin/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-cli.zip . && cd -
checkerror $? "Failed to pack"
cd "../private/BassBoom.Cli/bin/$releaseconf/net48/" && "$zippath" -r /tmp/$version-cli-48.zip . && cd -
checkerror $? "Failed to pack"

# Inform success
mv /tmp/$version-cli.zip .
checkerror $? "Failed to move archive from temporary folder"
mv /tmp/$version-cli-48.zip .
checkerror $? "Failed to move archive from temporary folder"
echo Build and pack successful.
exit 0
