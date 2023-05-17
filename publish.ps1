dotnet publish src -r win-x64   -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-win-x64.zip

dotnet publish src -r win-arm64 -c Release -o publish && 
    compress-archive -path .\publish\RollingBackupSweep.exe -destinationpath .\publish\RollingBackupSweep-win-arm64.zip