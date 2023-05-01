#!/usr/bin/env sh

if [ "$(whoami)" != "root" ]; then
  echo "error: root privilege required."
  exit 0
fi

PP=$PWD
CP=$(dirname "$(readlink -f "$0")")
cd "${CP}/BlogMan" || exit 1

dotnet publish -c Release -o ../publish

rm -rf /opt/blogman
mkdir /opt/blogman
mv ../publish/* /opt/blogman

ln -sf /opt/blogman/blogman /usr/local/bin/blogman

cd "${PP}"
