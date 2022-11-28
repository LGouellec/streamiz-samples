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