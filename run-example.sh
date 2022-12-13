#!/bin/bash

red=`tput setaf 1`
green=`tput setaf 2`
yellow=`tput setaf 3`
reset=`tput sgr0`

function prop() {
    grep "${2}" $1 | cut -d'=' -f2
}

POSITIONAL_ARGS=()

while [[ $# -gt 0 ]]; do
  case $1 in
    -e|--example)
      EXAMPLE="$2"
      shift # past argument
      shift # past value
      ;;
    -*|--*)
      echo "Unknown option $1"
      exit 1
      ;;
    *)
      POSITIONAL_ARGS+=("$1") # save positional arg
      shift # past argument
      ;;
  esac
done

set -- "${POSITIONAL_ARGS[@]}" # restore positional parameters

echo "${yellow}EXAMPLE                   = ${EXAMPLE}${reset}"

if [ -d "./src/${EXAMPLE}" ]; then
    config_file="./src/${EXAMPLE}/config-example.properties"
    override_docker=
    artifacts=
    root_directory=`pwd`
    result=

    if [ -f "${config_file}" ]; then
        override_docker=$(prop ${config_file} 'override.docker')
        artifacts=$(prop ${config_file} 'artifacts')
        result=$(prop ${config_file} 'result')
    fi

    echo "${yellow}OVERRIDE DOCKER           = ${override_docker}${reset}"
    echo "${yellow}ARTIFACTS                 = ${artifacts}${reset}"

    echo "${red}Stop and remove all docker container existing${reset}"
    docker rm -f $(docker ps -aq) > /dev/null 2>&1

    if [ "$override_docker" = "true" ]; then
        docker-compose -f "./environment/docker-compose.yml" -f "./src/${EXAMPLE}/docker-compose.override.yml" build
        docker-compose -f "./environment/docker-compose.yml" -f "./src/${EXAMPLE}/docker-compose.override.yml" up -d zookeeper broker schema-registry akhq
    else
        docker-compose -f "./environment/docker-compose.yml" up -d
    fi

    zookeeperContainerId=`docker ps -f name=zookeeper | tail -n 1 | awk '{print $1}'`
    kafkaContainerId=`docker ps -f name=broker | tail -n 1 | awk '{print $1}'`

    # Waiting zookeeper is UP
    echo "${green}Waiting zookeper ...${reset}"
    test=true
    while test
    do
        ret=`echo ruok | docker exec -i ${zookeeperContainerId} nc localhost 2181 | awk '{print $1}'`
        sleep 1
        echo "Waiting zookeeper UP"
        if $ret == 'imok'
        then
            test=false
        fi
    done

    # Wait broker is UP
    test=true
    echo "${green}Waiting kafka broker ...${reset}"
    while test
    do
        ret=`echo dump | docker exec -i ${zookeeperContainerId} nc localhost 2181 | grep brokers | wc -l`
        sleep 1
        echo "Waiting kafka UP"
        if $ret == 1
        then
            test=false
        fi
    done

    # Wait SR is UP
    echo "${green}Waiting schema registry ...${reset}"
    while [[ "$(curl -s -o /dev/null -w ''%{http_code}'' localhost:8081)" != "200" ]]; 
    do
      sleep 1;
    done

    if [ "./src/${EXAMPLE}/$artifacts" != "" ]; then
      
      while IFS= read -r line; do
        topic=`echo $line | cut -d':' -f1`
        partition=`echo $line | cut -d':' -f2`
        docker exec ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --topic $topic --create --partitions $partition --replication-factor 1 > /dev/null 2>&1
        echo "${green} Topic $topic created"
      done < "./src/${EXAMPLE}/$artifacts"
    fi

    echo "${reset}${yellow}List all topics ..."
    docker exec -i ${kafkaContainerId} kafka-topics --bootstrap-server broker:29092 --list --exclude-internal
    
    if [ "$override_docker" = "true" ]; then
        echo "${green}Start the rest of the stack ... ${reset}"
        docker-compose -f "./environment/docker-compose.yml" -f "./src/${EXAMPLE}/docker-compose.override.yml" up -d
    fi

    echo "${red}🚨 Example is READY .. Produce message into the source topic 💥 ${reset}"

    # Consume output topic for results
    if [ "${result}" != "" ]; then
      output_config_file="./src/${EXAMPLE}/${result}"
      sink_topic=$(prop ${output_config_file} 'topic')
      key_ser=$(prop ${output_config_file} 'key_deserializer')
      value_ser=$(prop ${output_config_file} 'value_deserializer')
      docker exec -it ${kafkaContainerId} kafka-console-consumer --bootstrap-server broker:29092 --topic ${sink_topic} --from-beginning --property print.key=true --property key.separator=" : " --key-deserializer ${key_ser} --value-deserializer ${value_ser}
    fi

else
    echo "${red}🚨 Example is not present in this repository${reset}"
fi