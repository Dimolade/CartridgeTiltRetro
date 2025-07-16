@echo off
setlocal
cd /d "%~dp0"
echo Running Dotnet
dotnet run --framework net8.0-windows > log.txt 2>&1
pause