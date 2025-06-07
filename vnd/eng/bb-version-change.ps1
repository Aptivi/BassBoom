$rootDir = $args[0]
$oldVersion = $args[1]
$newVersion = $args[2]

# Modify the Package.wxs file
$oldVersionSplit = $oldVersion.Split(".")
$newVersionSplit = $newVersion.Split(".")
$oldVerNoPatch = "$($oldVersionSplit[0]).$($oldVersionSplit[1]).x"
$newVerNoPatch = "$($newVersionSplit[0]).$($newVersionSplit[1]).x"
$oldPackageWxs = "Name=`"BassBoom $oldVerNoPatch`""
$newPackageWxs = "Name=`"BassBoom $newVerNoPatch`""
$packageWxsFile = "$rootDir\public\BassBoom.Installers\BassBoom.Installer\Package.wxs"
$packageWxsContent = [System.IO.File]::ReadAllText($packageWxsFile).Replace($oldPackageWxs, $newPackageWxs)
[System.IO.File]::WriteAllText($packageWxsFile, $packageWxsContent)

# Modify the PKGBUILD VCS files
$oldVerNoPatchSpec = "$($oldVersionSplit[0]).$($oldVersionSplit[1]).$($oldVersionSplit[2])"
$newVerNoPatchSpec = "$($newVersionSplit[0]).$($newVersionSplit[1]).$($newVersionSplit[2])"
$pkgBuildVcsFile = "$rootDir\PKGBUILD-VCS"
$oldPkgNameVcs = "pkgname=bassboom-$($oldApiVersionSplit[2])"
$newPkgNameVcs = "pkgname=bassboom-$($newApiVersionSplit[2])"
$oldPkgVerVcs = "pkgver=v$oldVerNoPatchSpec"
$newPkgVerVcs = "pkgver=v$newVerNoPatchSpec"
$oldBranchVcs = "branch=x/oob/v$oldVerNoPatch"
$newBranchVcs = "branch=x/oob/v$newVerNoPatch"
$pkgBuildVcsContent = [System.IO.File]::ReadAllText($pkgBuildVcsFile).Replace($oldPkgNameVcs, $newPkgNameVcs).Replace($oldPkgVerVcs, $newPkgVerVcs).Replace($oldBranchVcs, $newBranchVcs)
[System.IO.File]::WriteAllText($pkgBuildVcsFile, $pkgBuildVcsContent)
