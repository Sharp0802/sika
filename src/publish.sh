#!/usr/bin/env sh

PP=$PWD
CP=$(dirname "$(readlink -f "$0")")
cd "${CP}/SIKA" || exit 1

dotnet publish -r linux-x64      --self-contained -o publish-linux-x64
dotnet publish -r linux-musl-x64 --self-contained -o publish-linux-musl-x64
dotnet publish -r win-x64        --self-contained -o publish-win-x64
dotnet publish -r win-x86        --self-contained -o publish-win-x86

mkdir -p publish
cd publish

tar -lzma -cvf publish-linux-x64.tar.gz      ../publish-linux-x64      
tar -lzma -cvf publish-linux-musl-x64.tar.gz ../publish-linux-musl-x64 
tar -lzma -cvf publish-win-x64.tar.gz        ../publish-win-x64        
tar -lzma -cvf publish-win-x86.tar.gz        ../publish-win-x86        

rm -rf ../publish-linux-x64
rm -rf ../publish-linux-musl-x64
rm -rf ../publish-win-x64
rm -rf ../publish-win-x86

cd "${PP}"
