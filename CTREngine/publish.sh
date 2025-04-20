#!/bin/bash
dotnet publish -r win-x64 -c Release -o ./publish/win --self-contained true /p:PublishSingleFile=true
dotnet publish -r linux-x64 -c Release -o ./publish/linux --self-contained true /p:PublishSingleFile=true

