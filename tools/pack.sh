#!/bin/bash

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Vendor functions
packall() { return 0; }

# Sourcing the vendor script
export VENDOR_ERRORCODE=0
source $ROOTDIR/vnd/vendor.sh

# Convenience functions
checkerror() {
    if [ $1 != 0 ]
    then
        printf "$2 - Error $1\n" >&2
        exit $1
    fi
}

checkvendorerror() {
    if [ $VENDOR_ERRORCODE == 0 ]
    then
        export VENDOR_ERRORCODE=$1
    fi
}

# Pack all artifacts using vendor action
packall "$@"
checkerror $VENDOR_ERRORCODE "Failed to run artifact packing function from the vendor"

# Inform success
echo Pack successful.
