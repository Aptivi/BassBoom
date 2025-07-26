#!/bin/bash

localize() {
    # Check for dependencies
    dotnetpath=`which dotnet`
    checkerror $? "dotnet is not found"

    # Turn off telemetry and logo
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
    export DOTNET_NOLOGO=1

    # Restore the packages
    echo "Restoring NuGet packages..."
    "$dotnetpath" restore "$ROOTDIR/BassBoom.sln" --packages "$ROOTDIR/nuget"
    checkerror $? "Failed to restore NuGet packages"

    # Download libmpv for Windows
    echo "Downloading libmpv for Windows..."
    curl -L --output "$ROOTDIR/vnd/mpv-dev-x86_64-20250628-git-ed8954e.7z" https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-x86_64-20250628-git-ed8954e.7z
    curl -L --output "$ROOTDIR/vnd/mpv-dev-aarch64-20250628-git-ed8954e.7z" https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-aarch64-20250628-git-ed8954e.7z

    # Copy dependencies to the "deps" folder underneath the root directory
    mkdir -p "$ROOTDIR/deps"
    checkerror $? "Failed to initialize the dependencies folder"
    cp "$HOME/.nuget/packages"/*/*/*.nupkg "$ROOTDIR/deps/"
    cp "$ROOTDIR/nuget"/*/*/*.nupkg "$ROOTDIR/deps/"
    checkerror $? "Failed to vendor dependencies"
    rm -rf "$ROOTDIR/nuget"
    checkerror $? "Failed to wipe cache"

    # Initialize the NuGet configuration
    cat > "$ROOTDIR/NuGet.config" << EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="./deps" />
  </packageSources>
</configuration>
EOF
    checkerror $? "Failed to generate offline NuGet config"
}

prebuild() {
    # Check for dependencies
    dotnetpath=`which dotnet`
    checkerror $? "dotnet is not found"
    sevenzpath=`which 7z`
    checkerror $? "7z is not found"

    # Turn off telemetry and logo
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
    export DOTNET_NOLOGO=1

    # Download compiled Windows libmpv libraries
    if [ ! -f $ROOTDIR/vnd/mpv-dev-x86_64-20250628-git-ed8954e.7z ]; then
        curl -L --output $ROOTDIR/vnd/mpv-dev-x86_64-20250628-git-ed8954e.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-x86_64-20250628-git-ed8954e.7z
        checkvendorerror $?
    fi
    if [ ! -f $ROOTDIR/vnd/mpv-dev-aarch64-20250628-git-ed8954e.7z ]; then
        curl -L --output $ROOTDIR/vnd/mpv-dev-aarch64-20250628-git-ed8954e.7z https://github.com/zhongfly/mpv-winbuild/releases/download/2025-06-28-ed8954e/mpv-dev-aarch64-20250628-git-ed8954e.7z
        checkvendorerror $?
    fi

    # Install the DLL for AMD64
    cd $ROOTDIR/vnd && "$sevenzpath" x $ROOTDIR/vnd/mpv-dev-x86_64-20250628-git-ed8954e.7z libmpv-2.dll && cd -
    checkvendorerror $?
    mkdir -p $ROOTDIR/public/BassBoom.Native/runtimes/win-x64/native/
    checkvendorerror $?
    mv $ROOTDIR/vnd/libmpv-2.dll $ROOTDIR/public/BassBoom.Native/runtimes/win-x64/native/
    checkvendorerror $?
    
    # Install the DLL for ARM64
    cd $ROOTDIR/vnd && "$sevenzpath" x $ROOTDIR/vnd/mpv-dev-aarch64-20250628-git-ed8954e.7z libmpv-2.dll && cd -
    checkvendorerror $?
    mkdir -p $ROOTDIR/public/BassBoom.Native/runtimes/win-arm64/native/
    checkvendorerror $?
    mv $ROOTDIR/vnd/libmpv-2.dll $ROOTDIR/public/BassBoom.Native/runtimes/win-arm64/native/
    checkvendorerror $?
}

build() {
    # Determine the release configuration
    releaseconf=$1
    if [ -z $releaseconf ]; then
	    releaseconf=Release
    fi

    # Now, build.
    echo Building with configuration $releaseconf...
    "$dotnetpath" build "$ROOTDIR/BassBoom.sln" -p:Configuration=$releaseconf ${@:2}
    checkvendorerror $?
}

docpack() {
    # Get the project version
    version=$(grep "<Version>" $ROOTDIR/Directory.Build.props | cut -d "<" -f 2 | cut -d ">" -f 2)
    checkerror $? "Failed to get version. Check to make sure that the version is specified correctly in D.B.props"

    # Check for dependencies
    zippath=`which zip`
    checkerror $? "zip is not found"

    # Pack documentation
    echo Packing documentation...
    cd "$ROOTDIR/docs/" && "$zippath" -r /tmp/$version-doc.zip . && cd -
    checkvendorerror $?

    # Clean things up
    rm -rf "$ROOTDIR/DocGen/api"
    checkvendorerror $?
    rm -rf "$ROOTDIR/DocGen/obj"
    checkvendorerror $?
    rm -rf "$ROOTDIR/docs"
    checkvendorerror $?
    mv /tmp/$version-doc.zip "$ROOTDIR/tools"
    checkvendorerror $?
}

docgenerate() {
    # Check for dependencies
    docfxpath=`which docfx`
    checkerror $? "docfx is not found"

    # Turn off telemetry and logo
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
    export DOTNET_NOLOGO=1

    # Build docs
    echo Building documentation...
    "$docfxpath" $ROOTDIR/DocGen/docfx.json
    checkvendorerror $?
}

packall() {
    # Determine the release configuration
    releaseconf=$1
    if [ -z $releaseconf ]; then
	    releaseconf=Release
    fi
    
    # Get the project version
    version=$(grep "<Version>" $ROOTDIR/Directory.Build.props | cut -d "<" -f 2 | cut -d ">" -f 2)
    checkerror $? "Failed to get version. Check to make sure that the version is specified correctly in D.B.props"

    # Check for dependencies
    zippath=`which zip`
    checkerror $? "zip is not found"

    # Pack binary
    echo Packing binary with configuration $releaseconf...
    cd "$ROOTDIR/private/BassBoom.Cli/bin/$releaseconf/net8.0/" && "$zippath" -r /tmp/$version-cli.zip . && cd -
    checkvendorerror $?
    cd "$ROOTDIR/private/BassBoom.Cli/bin/$releaseconf/net48/" && "$zippath" -r /tmp/$version-cli-48.zip . && cd -
    checkvendorerror $?

    # Inform success
    mv /tmp/$version-cli.zip $ROOTDIR/tools/
    checkvendorerror $?
    mv /tmp/$version-cli-48.zip $ROOTDIR/tools/
    checkvendorerror $?
}

pushall() {
    # This script pushes.
    releaseconf=$1
    if [ -z $releaseconf ]; then
	    releaseconf=Release
    fi
    nugetsource=$2
    if [ -z $nugetsource ]; then
	    nugetsource=nuget.org
    fi
    dotnetpath=`which dotnet`
    checkerror $? "dotnet is not found"

    # Push packages
    echo Pushing packages with configuration $releaseconf to $nugetsource...
    packages=()
    while IFS= read -r pkg; do
        packages+=("$pkg")
    done < <(find "$ROOTDIR" -type f -path "*/bin/$releaseconf/*.nupkg")
    for pkg in "${packages[@]}"; do
        echo "$pkg"
        dotnet nuget push "$pkg" --api-key "$NUGET_APIKEY" --source "$nugetsource" --skip-duplicate
        push_result=$?
        if [ $push_result -ne 0 ]; then
            checkvendorerror $push_result
            return $push_result
        fi
    done
    
    checkvendorerror $?
}

increment() {
    # Check the versions
    OLDVER=$1
    NEWVER=$2
    OLDAPIVER=$3
    NEWAPIVER=$4
    if [ -z $OLDVER ]; then
        printf "Old version must be specified.\n"
        exit 1
    fi
    if [ -z $NEWVER ]; then
        printf "New version must be specified to replace old version $OLDVER.\n"
        exit 1
    fi
    if [ -z $OLDAPIVER ]; then
        printf "Old API version must be specified.\n"
        exit 1
    fi
    if [ -z $NEWAPIVER ]; then
        printf "New API version must be specified to replace old API version $NEWAPIVER.\n"
        exit 1
    fi

    # Populate some of the files needed to replace the old version with the new version
    FILES=(
        "$ROOTDIR/PKGBUILD-REL"
        "$ROOTDIR/.github/workflows/build-ppa-package-with-lintian.yml"
        "$ROOTDIR/.github/workflows/build-ppa-package.yml"
        "$ROOTDIR/.github/workflows/pushamend.yml"
        "$ROOTDIR/.github/workflows/pushppa.yml"
        "$ROOTDIR/.gitlab/workflows/release.yml"
        "$ROOTDIR/public/BassBoom.Installers/BassBoom.Installer/Package.wxs"
        "$ROOTDIR/public/BassBoom.Installers/BassBoom.InstallerBundle/Bundle.wxs"
        "$ROOTDIR/Directory.Build.props"
        "$ROOTDIR/CHANGES.TITLE"
    )
    IFS='.' read -ra APIVERSPLITOLD <<< "$OLDAPIVER"
    IFS='.' read -ra APIVERSPLITNEW <<< "$NEWAPIVER"
    for FILE in "${FILES[@]}"; do
        printf "Processing $FILE...\n"
        sed -b -i "s/$OLDVER/$NEWVER/g" "$FILE"
        sed -b -i "s/$OLDAPIVER/$NEWAPIVER/g" "$FILE"
        sed -b -i "s/bassboom-${APIVERSPLITOLD[2]}/bassboom-${APIVERSPLITNEW[2]}/g" "$FILE"
        result=$?
        if [ $result -ne 0 ]; then
            checkvendorerror $result
            return $result
        fi
    done

    # Modify the Package.wxs file
    IFS='.' read -ra VERSPLITOLD <<< "$OLDVER"
    IFS='.' read -ra VERSPLITNEW <<< "$NEWVER"
    OLDMAJOR="${VERSPLITOLD[0]}.${VERSPLITOLD[1]}.x"
    NEWMAJOR="${VERSPLITNEW[0]}.${VERSPLITNEW[1]}.x"
    sed -b -i "s/Name=\"BassBoom $OLDMAJOR\"/Name=\"BassBoom $NEWMAJOR\"/g" "$ROOTDIR/public/BassBoom.Installers/BassBoom.Installer/Package.wxs"

    # Modify the PKGBUILD VCS files
    OLDMAJORSPEC="${VERSPLITOLD[0]}.${VERSPLITOLD[1]}.${VERSPLITOLD[2]}"
    NEWMAJORSPEC="${VERSPLITNEW[0]}.${VERSPLITNEW[1]}.${VERSPLITNEW[2]}"
    sed -b -i "s/pkgname=bassboom-${APIVERSPLITOLD[2]}/pkgname=bassboom-${APIVERSPLITNEW[2]}/g" "$ROOTDIR"/PKGBUILD-VCS*
    sed -b -i "s/pkgver=v$OLDMAJORSPEC/pkgver=v$NEWMAJORSPEC/g" "$ROOTDIR"/PKGBUILD-VCS*
    sed -b -i "s/branch=x\/oob\/v$OLDMAJOR/branch=x\/oob\/v$NEWMAJOR/g" "$ROOTDIR"/PKGBUILD-VCS*

    # Add a Debian changelog entry
    printf "Changing Debian changelogs info...\n"
    DEBIAN_CHANGES_FILE="$ROOTDIR/debian/changelog"
    DEBIAN_CHANGES_DATE=$(date "+%a, %d %b %Y %H:%M:%S %z")
    DEBIAN_CHANGES_ENTRY=$(cat <<EOF
bassboom-${APIVERSPLITNEW[2]} ($NEWAPIVER-$NEWVER-1) noble; urgency=medium

  * Please populate changelogs here

 -- Aptivi CEO <ceo@aptivi.anonaddy.com>  $DEBIAN_CHANGES_DATE
EOF
    )
    DEBIAN_CHANGES_CONTENT=$(printf "$DEBIAN_CHANGES_ENTRY\n\n$(cat "$DEBIAN_CHANGES_FILE")")
    printf "$DEBIAN_CHANGES_CONTENT\n" > $DEBIAN_CHANGES_FILE
}

clean() {
    OUTPUTS=(
        '-name "bin" -or'
        '-name "obj" -or'
        '-name "bassboom-3" -or'
        '-name "tmp" -or'
        '-name "docs"'
    )
    find "$ROOTDIR" -type d \( ${OUTPUTS[@]} \) -print -exec rm -rf "{}" +
}
