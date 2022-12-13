ARG CONNECT_USER
FROM confluentinc/cp-server-connect-base:7.2.1
ARG TAG
ARG CONNECT_USER
USER root
RUN mkdir -p /usr/share/confluent-hub-components && chown -R ${CONNECT_USER}:${CONNECT_USER} /usr/share/confluent-hub-components /etc/kafka /etc/schema-registry
USER ${CONNECT_USER}

RUN confluent-hub install --no-prompt confluentinc/kafka-connect-datagen:latest