#!/bin/bash

echo "Stop and remove all docker container existing"
docker rm -f $(docker ps -aq) > /dev/null 2>&1

# Build images
docker-compose -f "docker-compose.yml" build

# Start broker, zk, connect and akhq
docker-compose -f "docker-compose.yml" up -d zookeeper broker akhq connect

# Get the kafka container id
kafkaContainerId=`docker ps -f name=broker | tail -n 1 | awk '{print $1}'`

# Create intput and output topics
docker exec ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --topic customers --create --partitions 3 --replication-factor 1 > /dev/null 2>&1
docker exec ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --topic orders --create --partitions 10 --replication-factor 1 > /dev/null 2>&1
docker exec ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --topic products --create --partitions 3 --replication-factor 1 > /dev/null 2>&1
docker exec ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --topic orders-enriched --create --partitions 10 --replication-factor 1 > /dev/null 2>&1

# Start the rest of the stack
docker-compose -f "docker-compose.yml" up -d

echo "🚀 The deployment is complete. Please run the ./start-connectors.sh .. "

# Consume output records
docker exec ${kafkaContainerId} kafka-console-consumer --bootstrap-server broker:29092 --topic orders-enriched --property print.key=true --value-deserializer org.apache.kafka.connect.json.JsonDeserializer