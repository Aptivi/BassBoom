#!/bin/bash

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Vendor functions
docpack() { return 0; }

# Sourcing the vendor script
export VENDOR_ERRORCODE=0
source $ROOTDIR/vendor.sh

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

# Pack using vendor action
docpack $@
checkerror $VENDOR_ERRORCODE "Failed to run documentation pack function from the vendor"

# Inform success
echo Pack successful.
