cd $PSScriptRoot

Write-Host "### Setting values"
$Version = ([Xml] (Get-Content .\WrocWithoutQueueCheck.csproj)).Project.PropertyGroup.Version
Write-Host "Version = $Version"
$WinPath = "./publish/WrocWithoutQueueCheck_$($Version)_win-x64"
Write-Host "WinPath = $WinPath"
$LinuxPath = "./publish/WrocWithoutQueueCheck_$($Version)_linux-x64"
Write-Host "LinuxPath = $LinuxPath"

Write-Host "### Removing old publish if exists"
if (Test-Path $WinPath) { Remove-Item -Recurse -Force $WinPath }
if (Test-Path "$WinPath.zip") { Remove-Item -Recurse -Force "$WinPath.zip" }
if (Test-Path $LinuxPath) { Remove-Item -Recurse -Force $LinuxPath }
if (Test-Path "$LinuxPath.zip") { Remove-Item -Recurse -Force "$LinuxPath.zip" }

Write-Host "### Publishing win-x64..."
dotnet publish --output $WinPath --runtime win-x64 --self-contained
if (Test-Path "$WinPath/smtp.json") { Remove-Item "$WinPath/smtp.json" }
Compress-Archive $WinPath/* "$WinPath.zip"

Write-Host "### Publishing linux-x64..."
dotnet publish --output $LinuxPath --runtime linux-x64 --self-contained
if (Test-Path "$LinuxPath/smtp.json") { Remove-Item "$LinuxPath/smtp.json" }
Compress-Archive $LinuxPath/* "$LinuxPath.zip"