#!/bin/bash

# Create referentials data
curl -s -X PUT \
      -H "Content-Type: application/json" \
      --data '{
                "connector.class": "io.confluent.kafka.connect.datagen.DatagenConnector",
                "kafka.topic": "customers",
                "max.interval": 1,
                "quickstart": "shoe_customers",
                "iterations": 1000,
                "tasks.max": "1",
                "transforms": "ValueToKey,ExtractField",
                "transforms.ValueToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
                "transforms.ValueToKey.fields": "id",
                "transforms.ExtractField.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
                "transforms.ExtractField.field": "id"
            }' \
      http://localhost:8083/connectors/datagen-customers/config


curl -s -X PUT \
      -H "Content-Type: application/json" \
      --data '{
                "connector.class": "io.confluent.kafka.connect.datagen.DatagenConnector",
                "kafka.topic": "products",
                "max.interval": 1,
                "iterations": 1000,
                "quickstart": "shoes",
                "tasks.max": "1",
                "transforms": "ValueToKey,ExtractField",
                "transforms.ValueToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
                "transforms.ValueToKey.fields": "id",
                "transforms.ExtractField.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
                "transforms.ExtractField.field": "id"
            }' \
      http://localhost:8083/connectors/datagen-products/config

sleep 5;

# Start connectors for mock input data
curl -s -X PUT \
      -H "Content-Type: application/json" \
      --data '{
                "connector.class": "io.confluent.kafka.connect.datagen.DatagenConnector",
                "kafka.topic": "orders",
                "max.interval": 50,
                "quickstart": "shoe_orders",
                "tasks.max": "1",
                "transforms": "ValueToKey,ExtractField",
                "transforms.ValueToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
                "transforms.ValueToKey.fields": "customer_id",
                "transforms.ExtractField.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
                "transforms.ExtractField.field": "customer_id"
            }' \
      http://localhost:8083/connectors/datagen-orders/config