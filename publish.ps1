if([String]::IsNullOrEmpty($Args[0])) { Write-Host "Version is required"; exit 1; }
$version = $Args[0];

dotnet publish src -r win-x64   -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-${version}-win-x64.zip

dotnet publish src -r win-arm64 -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-${version}-win-arm64.zip

$yesno = Read-Host -Prompt "Did you build linux first? Tag git, push all; create ${version} at github and upload? [Y/N]"
if ($yesno -ieq 'y') {
    git tag $version
    git push --all
    gh release create $version 
    gh release upload $version RollingBackupSweep-${version}-linux-arm64.tar RollingBackupSweep-${version}-linux-amd64.tar RollingBackupSweep-${version}-win-x64.tar RollingBackupSweep-${version}-win-x64.tar
}