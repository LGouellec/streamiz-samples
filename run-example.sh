#!/bin/bash

Folder of each specific example
Parameters = name of example which name of example Folder
Run .net sample or just deploy the stack

Config file
- override docker-compose-file = yes | false (default : false)
- artifacts-file=../ (default: empty)

if (run .net){
    dotnet restore
    dotnet build
    dotnet run ...
}
