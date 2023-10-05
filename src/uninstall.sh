#!/usr/bin/env sh

if [ "$(whoami)" != "root" ]; then
  echo "error: root privilege required."
  exit 0
fi

#PP=$PWD
#CP=$(dirname "$(readlink -f "$0")")
#cd "${CP}/BlogMan" || exit 1

rm -rf /opt/sika           || exit 2
unlink /usr/local/bin/sika || exit 2

#cd "${PP}"
