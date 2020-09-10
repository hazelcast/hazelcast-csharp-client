#!/bin/sh

POWERSHELL=powershell
if ! type $POWERSHELL >/dev/null 2>&1; then
  POWERSHELL=pwsh
fi
if ! type $POWERSHELL >/dev/null 2>&1; then
  POWERSHELL=""
fi
if [ -z $POWERSHELL ]; then
  echo "Could not find a 'powershell' or 'pwsh' command."
  echo "Please make sure that Powershell is installed."
  exit 1
fi

$POWERSHELL build/build.ps1 "$@"

#eof
