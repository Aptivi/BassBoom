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

# This script builds and packs the artifacts. Use when you have MSBuild installed.
version=$(grep "<Version>" ../Directory.Build.props | cut -d "<" -f 2 | cut -d ">" -f 2)
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi

# Check for dependencies
zippath=`which zip`
if [ ! $? == 0 ]; then
	echo zip is not found.
	exit 1
fi

# Pack binary
echo Packing binary...
cd "../BassBoom.Basolia/bin/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-api.zip . && cd -
cd "../BassBoom/bin.Cli/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-protocli.zip . && cd -
cd "../BassBoom/bin.Native/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-unsafeapi.zip . && cd -
cd "../BassBoom.Basolia/bin/$releaseconf/net48/" && "$zippath" -r /tmp/$version-api-48.zip . && cd -
cd "../BassBoom/bin.Cli/$releaseconf/net48/" && "$zippath" -r /tmp/$version-protocli-48.zip . && cd -
cd "../BassBoom/bin.Native/$releaseconf/net48/" && "$zippath" -r /tmp/$version-unsafeapi-48.zip . && cd -
if [ ! $? == 0 ]; then
	echo Packing using zip failed.
	exit 1
fi

# Inform success
mv ~/tmp/$version-api.zip .
mv ~/tmp/$version-protocli.zip .
mv ~/tmp/$version-unsafeapi.zip .
mv ~/tmp/$version-api-48.zip .
mv ~/tmp/$version-protocli-48.zip .
mv ~/tmp/$version-unsafeapi-48.zip .
echo Build and pack successful.
exit 0
