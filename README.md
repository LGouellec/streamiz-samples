# kafka-streams-dotnet-samples

Demo applications and code examples for Kafka Streams Dotnet API.


See: https://github.com/confluentinc/kafka-streams-examples/pull/363

TODO : Create some examples Kafka Streams with .NET implementations


Folder of each specific example
Parameters = name of example which name of example Folder
Run .net sample or just deploy the stack

Config file
- override docker-compose-file = yes | false (default : false)
- artifacts-file=../ (default: empty)

Stack is ready, we will start .net stream processing application, after starting you can open one new terminal to produce message and one (or multiple) to consume output messages
List of all topics implicated in this examples
...
...
...

if (run .net){
    dotnet restore
    dotnet build
    dotnet run ...
}


# EXAMPE MAP FUNCTION
seq -f "key:value%g" 10 | docker exec -i broker kafka-console-producer --bootstrap-server broker:29092 --topic text-lines-topic --property parse.key=true --property key.separator=:

EXPECTED
key : VALUE1
key : VALUE2
key : VALUE3
key : VALUE4
key : VALUE5
key : VALUE6
key : VALUE7
key : VALUE8
key : VALUE9
key : VALUE10


# EXAMPLE WORD COUNT 

for i in {1..10}; do echo key:value;done | docker exec -i broker kafka-console-producer --bootstrap-server broker:29092 --topic plaintext-input --property parse.key=true --property key.separator=:

EXPECTED
value : 1
value : 2
value : 3
value : 4
value : 5
value : 6
value : 7
value : 8
value : 9
value : 10

# EXAMPLE SUM

seq -f "%g" 10 | docker exec -i broker kafka-console-producer --bootstrap-server broker:29092 --topic numbers-topic --property parse.key=false

EXPECTED : 
1 : 1
1 : 3
1 : 6
1 : 10
1 : 15
1 : 21
1 : 28
1 : 36
1 : 45
1 : 55

# EXAMPE GLOBAL-KTABLE

Product
seq -f "1|{\"name\": \"iPhone %g\", \"id\": 1}" 10 10 | docker exec -i schema-registry kafka-avro-console-producer --bootstrap-server broker:29092 --property schema.registry.url=http://localhost:8081 --topic product --property value.schema='{"type":"record","name":"Product","namespace":"com.dotnet.samples.avro","fields":[{"name":"id","type":"int"},{"name":"name","type":"string"}]}' --property parse.key=true --property key.separator=\| --property key.serializer=org.apache.kafka.common.serialization.StringSerializer

Customer
seq -f "1|{\"name\": \"Customer %g\", \"id\": 1}" 1 1 | docker exec -i schema-registry kafka-avro-console-producer --bootstrap-server broker:29092 --property schema.registry.url=http://localhost:8081 --topic customer --property value.schema='{"type":"record","name":"Customer","namespace":"com.dotnet.samples.avro","fields":[{"name":"id","type":"int"},{"name":"name","type":"string"}]}' --property parse.key=true --property key.separator=\| --property key.serializer=org.apache.kafka.common.serialization.StringSerializer

Publish an order
for i in {1..3}; do echo "${i}|{\"productId\": 1, \"id\": ${i}, \"customerId\": 1}";done | docker exec -i schema-registry kafka-avro-console-producer --bootstrap-server broker:29092 --property schema.registry.url=http://localhost:8081 --topic order --property value.schema='{"type":"record","name":"Order","namespace":"com.dotnet.samples.avro","fields":[{"name":"id","type":"int"},{"name":"customerId","type":"int"},{"name":"productId","type":"int"}]}' --property parse.key=true --property key.separator=\| --property key.serializer=org.apache.kafka.common.serialization.StringSerializer


EXPECTED (Order can differed)
2 : {"productName":"iPhone 10","productId":1,"customerName":"Customer 1","customerId":1,"orderId":2}
3 : {"productName":"iPhone 10","productId":1,"customerName":"Customer 1","customerId":1,"orderId":3}
1 : {"productName":"iPhone 10","productId":1,"customerName":"Customer 1","customerId":1,"orderId":1

# EXAMPE PAGE-VIEW-REGION

Customer
echo "User1|{\"user\": \"User1\", \"region\": \"usa-central\"}\nUser2|{\"user\": \"User2\", \"region\": \"usa-west\"}\nUser3|{\"user\": \"User3\", \"region\": \"france\"}" | docker exec -i schema-registry kafka-avro-console-producer --bootstrap-server broker:29092 --property schema.registry.url=http://localhost:8081 --topic user-profiles --property value.schema='{"type":"record","name":"UserProfile","namespace":"com.dotnet.samples.avro","fields":[{"name":"user","type":"string"},{"name":"region","type":"string"}]}' --property parse.key=true --property key.separator=\| --property key.serializer=org.apache.kafka.common.serialization.StringSerializer

Publish an page view
echo "User1|{\"user\": \"User1\", \"url\": \"https://www.google.com\"}\nUser2|{\"user\": \"User2\", \"url\": \"https://www.amazon.com\"}\nUser3|{\"user\": \"User3\", \"url\": \"https://www.myshop.com\"}" | docker exec -i schema-registry kafka-avro-console-producer --bootstrap-server broker:29092 --property schema.registry.url=http://localhost:8081 --topic page-views --property value.schema='{"type":"record","name":"PageView","namespace":"com.dotnet.samples.avro","fields":[{"name":"user","type":"string"},{"name":"url","type":"string"}]}' --property parse.key=true --property key.separator=\| --property key.serializer=org.apache.kafka.common.serialization.StringSerializer
