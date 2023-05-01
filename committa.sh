#!/bin/bash

if [ -z "$1" ]
  then
    msg="Update on `date -u '+%Y-%m-%d %H:%M:%S UTC'`"
  else
    msg="$1"
fi

git pull && git add . && git commit -m "$msg" && git push && git status
