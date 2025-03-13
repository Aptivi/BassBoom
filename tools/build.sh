#!/bin/bash

# Repository root
ROOTDIR=$( cd -- "$( dirname -- "$0" )/.." &> /dev/null && pwd )

# Vendor functions
prebuild() { return 0; }
build() { return 0; }
postbuild() { return 0; }

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

# Run any vendor actions before build
prebuild "$@"
checkerror $VENDOR_ERRORCODE "Failed to run prebuild function from the vendor"

# Build using vendor action
build "$@"
checkerror $VENDOR_ERRORCODE "Failed to run build function from the vendor"

# Run any vendor actions after build
postbuild "$@"
checkerror $VENDOR_ERRORCODE "Failed to run postbuild function from the vendor"

# Inform success
echo Build successful.
