#!/bin/bash
if [[ $1 -eq 0 ]] 
then
    echo 'no version passed, will only tag with :latest'
    docker build -t boardz-api:latest .
else    
    echo 'will tag with :latest and $($1)'
    docker build -t boardz-api:$1 -t boardz-api:latest .
fi


