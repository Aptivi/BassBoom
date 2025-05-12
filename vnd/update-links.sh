#!/bin/sh

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Convenience functions
checkerror() {
    if [ "$1" != 0 ]
    then
        printf "%s - Error %s\n" "$2" "$1" >&2
        exit "$1"
    fi
}

# Check for dependencies
jqpath=$(which jq)
checkerror $? "jq is not found"
sedpath=$(which sed)
checkerror $? "sed is not found"

# Below variables to replace when script is complete
OLDREV="2025-05-12-b3070d1"
OLDFILENAMESTD="mpv-dev-x86_64-20250512-git-b3070d1.7z"
OLDFILENAMEARM="mpv-dev-aarch64-20250512-git-b3070d1.7z"

# Get the new links
echo "Checking for updates..."
RELAPILINK="https://api.github.com/repos/zhongfly/mpv-winbuild/releases/latest"
APIRESPONSE=$(curl -s $RELAPILINK || checkerror $? "Failed to download latest release info")

# Use jq to parse the JSON to extract important information
NEWREV=$(echo "$APIRESPONSE" | "$jqpath" -r '.tag_name')
NEWREVDATE=${NEWREV:0:10}
NEWREVGIT=${NEWREV:11}
NEWFILENAMESTD=$(echo "$APIRESPONSE" | "$jqpath" -r ".assets[].name | select(startswith(\"mpv-dev-x86_64-${NEWREVDATE//-/}\"))")
NEWFILENAMEARM=$(echo "$APIRESPONSE" | "$jqpath" -r ".assets[].name | select(startswith(\"mpv-dev-aarch64-${NEWREVDATE//-/}\"))")

# Use the new revision and the new file name to replace the old revision and file name in vendor scripts
if [ "$OLDREV" == "$NEWREV" ]; then
    echo "Already up to date"
    exit 0
fi
echo "Updating links from $OLDREV to $NEWREV"
echo "  - $OLDFILENAMESTD -> $NEWFILENAMESTD"
echo "  - $OLDFILENAMEARM -> $NEWFILENAMEARM"
find "$ROOTDIR/vnd" -type f -name "vendor*" -exec sed -i "s/$OLDREV/$NEWREV/g" "{}" \;
find "$ROOTDIR/vnd" -type f -name "vendor*" -exec sed -i "s/$OLDFILENAMESTD/$NEWFILENAMESTD/g" "{}" \;
find "$ROOTDIR/vnd" -type f -name "vendor*" -exec sed -i "s/$OLDFILENAMEARM/$NEWFILENAMEARM/g" "{}" \;
sed -i "s/OLDREV=\"$OLDREV\"/OLDREV=\"$NEWREV\"/g" "$ROOTDIR/vnd/update-links.sh"
sed -i "s/OLDFILENAMESTD=\"$OLDFILENAMESTD\"/OLDFILENAMESTD=\"$NEWFILENAMESTD\"/g" "$ROOTDIR/vnd/update-links.sh"
sed -i "s/OLDFILENAMEARM=\"$OLDFILENAMEARM\"/OLDFILENAMEARM=\"$NEWFILENAMEARM\"/g" "$ROOTDIR/vnd/update-links.sh"
