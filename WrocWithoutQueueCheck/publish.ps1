Write-Host "### Removing old publish..."
Remove-Item -Recurse -Force ./publish
Write-Host "### Building win-x64..."
dotnet publish --output ./publish/win-x64 	--runtime win-x64 	--self-contained
Write-Host "### Building linux-x64..."
dotnet publish --output ./publish/linux-x64 --runtime linux-x64 --self-contained