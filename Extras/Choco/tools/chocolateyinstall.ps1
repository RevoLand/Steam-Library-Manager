$ErrorActionPreference = 'Stop'
 
$toolsPath  = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
 
$packageArgs = @{
    PackageName  = "steamlibrarymanager"
    File         = "$toolsPath\Steam.Library.Manager.zip"
    Destination  = $toolsPath
}
Get-ChocolateyUnzip @packageArgs
 
Remove-Item -force "$toolsPath\*.zip" -ea 0