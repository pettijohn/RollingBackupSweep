$ErrorActionPreference = 'Stop'

if([String]::IsNullOrEmpty($Args[0])) { Write-Host "Version is required"; exit 1; }
$version = $Args[0];
# Trim starting v --> v0.9.9 --> 0.9.9 because csproj version must be specific x.y.m.n format 
$versionNum = $version.TrimStart('v')

$projPath = get-item "src\RollingBackupSweep.csproj"
$csproj = [xml] (get-content $projPath)
$csproj.Project.PropertyGroup.Version = $versionNum
$csProj.Save($projPath)

wsl -- ./publish.sh $version

dotnet pack -c Release -o .\out\
# dotnet nuget push .\out\TaskSchedulerEngine.nupkg


dotnet publish src -r win-x64   -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-${version}-win-x64.zip

dotnet publish src -r win-arm64 -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-${version}-win-arm64.zip

$yesno = Read-Host -Prompt "Tag git, push all, push tags; create ${version} at github and upload? [Y/N]"
if ($yesno -ieq 'y') {
    git tag $version --force
    git push --all
    git push --tags --force 
    gh release create $version 
    pushd publish
    gh release upload $version RollingBackupSweep-${version}-linux-arm64.tar.gz RollingBackupSweep-${version}-linux-x64.tar.gz RollingBackupSweep-${version}-win-arm64.zip RollingBackupSweep-${version}-win-x64.zip
    popd 
}