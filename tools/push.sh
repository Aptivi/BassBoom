#!/bin/bash

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Vendor functions
pushall() { return 0; }

# Convenience functions
checkerror() {
    if [ $1 != 0 ]
    then
        printf "$2 - Error $1\n" >&2
        exit $1
    fi
}

# Sourcing the vendor script
export VENDOR_ERRORCODE=0
source $ROOTDIR/vnd/vendor.sh
checkerror $VENDOR_ERRORCODE "Failed to source the vendor script"

# Vendor error function
checkvendorerror() {
    if [ $VENDOR_ERRORCODE == 0 ]
    then
        export VENDOR_ERRORCODE=$1
    fi
}

# Push all artifacts using vendor action
pushall "$@"
checkerror $VENDOR_ERRORCODE "Failed to run artifact pushing function from the vendor"

# Inform success
echo Push successful.
