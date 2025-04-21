@echo off
setlocal
echo Publishing for Windows (win-x64)...
dotnet publish -r win-x64 -c Release -o .\publish\win --self-contained true -p:PublishSingleFile=true --framework net8.0-windows

echo.
echo Publishing for Linux (linux-x64)...
dotnet publish -r linux-x64 -c Release -o .\publish\linux --self-contained true -p:PublishSingleFile=true --framework net8.0

echo.
echo Success! Binaries are in the 'publish' folder.
pause

