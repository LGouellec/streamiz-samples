#!/bin/bash

echo "Destroy all docker containers"

docker rm -f $(docker ps -aq) > /dev/null 2>&1