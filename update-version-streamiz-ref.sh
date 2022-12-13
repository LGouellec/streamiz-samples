#!/bin/bash

files=`find src -type f -name "*.csproj"`

for file in ${files[@]}; do
  echo $file
  sed -i '' -e "s/Include=\"Streamiz.Kafka.Net\(.*\)\" Version=\"\(.*\)\"/Include=\"Streamiz.Kafka.Net\1\" Version=\"$1\"/g" $file
done