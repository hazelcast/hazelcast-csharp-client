#!/bin/sh -l

# run the hz script
echo "> hz $INPUT_ARGS"
pwsh ./hz.ps1 $INPUT_ARGS

# report result
exit $?