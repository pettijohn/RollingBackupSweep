dotnet publish src -r linux-x64   -c Release -o publish && tar -czf ./publish/RollingBackupSweep-linux-x64.tar.gz -C publish RollingBackupSweep

# Docker images https://github.com/dotnet/runtime/blob/main/docs/workflow/building/coreclr/linux-instructions.md#Docker-Images
# Ubuntu 22.04 https://github.com/dotnet/dotnet-buildtools-prereqs-docker/blob/main/src/ubuntu/22.04/cross/arm64/Dockerfile
# Extra compiler flags from https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/compiling.md

docker build -t dotnet-linux-arm64-cross - < Dockerfile-linux-arm64-cross
docker run -it --rm -v .:/code -v /usr/share/dotnet:/runtime -w /code dotnet-linux-arm64-cross \
    /runtime/dotnet publish src -r linux-arm64 -c Release -o publish \
    -p:CppCompilerAndLinker=clang -p:SysRoot=/crossrootfs/arm64 \
    && tar -czf ./publish/RollingBackupSweep-linux-arm64.tar.gz -C publish RollingBackupSweep