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
