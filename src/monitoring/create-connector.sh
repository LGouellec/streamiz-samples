#!/bin/bash

curl -s -X PUT \
      -H "Content-Type: application/json" \
      --data '{
                "connector.class": "io.confluent.kafka.connect.datagen.DatagenConnector",
                "kafka.topic": "input",
                "max.interval": 5,
                "quickstart": "pageviews",
                "tasks.max": "1",
                "transforms": "ValueToKey,ExtractField",
                "transforms.ValueToKey.type": "org.apache.kafka.connect.transforms.ValueToKey",
                "transforms.ValueToKey.fields": "userid",
                "transforms.ExtractField.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
                "transforms.ExtractField.field": "userid"
            }' \
      http://localhost:8083/connectors/datagen-pageviews/config