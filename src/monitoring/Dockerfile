FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["monitoring.csproj", "monitoring/"]
RUN dotnet restore "monitoring/monitoring.csproj"
COPY . monitoring/
WORKDIR "/src/monitoring"
RUN dotnet build "monitoring.csproj" -c Release -o /app/build

FROM build AS publish
ARG ROCKSDB_VERSION=v7.4.3
RUN apk add librdkafka --repository=https://dl-cdn.alpinelinux.org/alpine/v3.21/community/

# Workaround https://github.com/confluentinc/librdkafka/issues/4897
RUN cp /usr/lib/librdkafka.so.1 /app/alpine-librdkafka.so
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so
RUN dotnet publish "monitoring.csproj" -c Release -o /app/publish
ENTRYPOINT ["dotnet", "/app/publish/monitoring.dll"]
