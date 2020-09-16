#!/usr/bin/bash

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

# put quotes around args that contain whitespaces,
# this black magic thing works in bash on Windows and Linux,
# though I would have a hard time explaining why exactly.

c=()
for i in "$@"
do
	if [[ "$OS" == "Windows_NT" ]] && [[ "$i" =~ " " ]];
	then
    	c+=("\"$i\"")
	else
		c+=("$i")
	fi
done

$POWERSHELL ./hz.ps1 "${c[@]}"

#eof
