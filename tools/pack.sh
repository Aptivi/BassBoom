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
version=$(cat version)
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
cd "../BassBoom/bin/$releaseconf/net7.0/" && "$zippath" -r /tmp/$version-bin.zip . && cd -
cd "../BassBoom.Basolia/bin/$releaseconf/net7.0/" && "$zippath" -r /tmp/$version-api.zip . && cd -
cd "../BassBoom/bin.Desktop/$releaseconf/net7.0/" && "$zippath" -r /tmp/$version-proto.zip . && cd -
cd "../BassBoom/bin.Cli/$releaseconf/net7.0/" && "$zippath" -r /tmp/$version-protocli.zip . && cd -
cd "../BassBoom/bin.Native/$releaseconf/net7.0/" && "$zippath" -r /tmp/$version-unsafeapi.zip . && cd -
cd "../BassBoom/bin/$releaseconf/net7.0-windows/" && "$zippath" -r /tmp/$version-bin-win.zip . && cd -
cd "../BassBoom.Basolia/bin/$releaseconf/net7.0-windows/" && "$zippath" -r /tmp/$version-api-win.zip . && cd -
cd "../BassBoom/bin.Desktop/$releaseconf/net7.0-windows/" && "$zippath" -r /tmp/$version-proto-win.zip . && cd -
cd "../BassBoom/bin.Cli/$releaseconf/net7.0-windows/" && "$zippath" -r /tmp/$version-protocli-win.zip . && cd -
cd "../BassBoom/bin.Native/$releaseconf/net7.0-windows/" && "$zippath" -r /tmp/$version-unsafeapi-win.zip . && cd -
if [ ! $? == 0 ]; then
	echo Packing using zip failed.
	exit 1
fi

# Inform success
mv ~/tmp/$version-bin.zip .
mv ~/tmp/$version-api.zip .
mv ~/tmp/$version-proto.zip .
mv ~/tmp/$version-unsafeapi.zip .
mv ~/tmp/$version-bin-win.zip .
mv ~/tmp/$version-api-win.zip .
mv ~/tmp/$version-proto-win.zip .
mv ~/tmp/$version-protocli-win.zip .
mv ~/tmp/$version-unsafeapi-win.zip .
echo Build and pack successful.
exit 0
