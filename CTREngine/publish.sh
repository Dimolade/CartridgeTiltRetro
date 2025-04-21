#!/bin/bash
dotnet publish -r win-x64 -c Release -o ./publish/win --self-contained true /p:PublishSingleFile=true --framework net8.0-windows
dotnet publish -r linux-x64 -c Release -o ./publish/linux --self-contained true /p:PublishSingleFile=true --framework net8.0

