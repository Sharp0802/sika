#!/usr/bin/env sh

if [ "$(whoami)" != "root" ]; then
  echo "error: root privilege required."
  exit 0
fi

PP=$PWD
CP=$(dirname "$(readlink -f "$0")")
cd "${CP}/SIKA" || exit 1

dotnet publish -c Release -o ../publish

rm -rf /opt/sika
mkdir /opt/sika
mv ../publish/* /opt/sika

ln -sf /opt/sika/sika /usr/local/bin/sika

cd "${PP}"
