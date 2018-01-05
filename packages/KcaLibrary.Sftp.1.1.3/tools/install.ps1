###############################################################################
#
# install.ps1 --
#
# Written by Joe Mistachkin.
# Released to the public domain, use at your own risk!
#
###############################################################################

param($installPath, $toolsPath, $package, $project)


$fileName1 = "WinSCPnet.dll"
$fileName2 = "WinSCP.exe"
$propertyName = "CopyToOutputDirectory"


  $folder = $project.ProjectItems.Item("Lib")

  if ($folder -eq $null) {
  	"No Folder"
    continue
  }

  $item = $folder.ProjectItems.Item($fileName1)

  if ($item -eq $null) {
  	"No File 1"
    continue
  }

  $property = $item.Properties.Item($propertyName)

  if ($property -eq $null) {
    continue
  }

  $property.Value = 1
  
  $item = $folder.ProjectItems.Item($fileName2)

  if ($item -eq $null) {
   "No File 2"
    continue
  }

  $property = $item.Properties.Item($propertyName)

  if ($property -eq $null) {
    continue
  }

  $property.Value = 1

