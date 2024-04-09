#!/bin/sh

dotnet build -c Release -p:TargetFrameworks="net6.0" -p:PackageVersion=$1 -p:Version=$1 -p:FileVersion=$1 ASK.HAL/ASK.HAL.csproj 
dotnet build -c Release -p:TargetFrameworks="net6.0" -p:PackageVersion=$1 -p:Version=$1 -p:FileVersion=$1 ASK.HAL.Mvc/ASK.HAL.Mvc.csproj
dotnet build -c Release -p:TargetFrameworks="net6.0" -p:PackageVersion=$1 -p:Version=$1 -p:FileVersion=$1 ASK.HAL/ASK.HAL.csproj 
dotnet build -c Release -p:TargetFrameworks="net8.0" -p:PackageVersion=$1 -p:Version=$1 -p:FileVersion=$1 ASK.HAL.Mvc/ASK.HAL.Mvc.csproj

rm -rf nuget
dotnet pack -c Release -p:PackageVersion=$1 -p:Version=$1 -p:FileVersion=$1 -o nuget
