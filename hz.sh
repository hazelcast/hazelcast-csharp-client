#!/usr/bin/bash

POWERSHELL=pwsh
if ! type $POWERSHELL >/dev/null 2>&1; then
  echo "Could not find the 'pwsh' command."
  echo "Please make sure that Powershell is installed."
  exit 1
fi

# put quotes around args that contain whitespaces,
# this black magic thing works in bash on Windows and Linux,
# though I would have a hard time explaining why exactly.

c=()
for i in "$@"; do if [ "$i" == "---" ]; then c+=("--%"); else c+=("$i"); fi done
$POWERSHELL ./hz.ps1 "${c[@]}"

#eof
