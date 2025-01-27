FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["word-count.csproj", "word-count/"]
RUN dotnet restore "word-count/word-count.csproj"
COPY ["Program.cs", "word-count/"]
WORKDIR "/src/word-count"
RUN dotnet build "word-count.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "word-count.csproj" -c Release -o /app/publish

FROM base AS final
ARG ROCKSDB_VERSION=v7.4.3
WORKDIR /app
RUN apk add librdkafka --repository=https://dl-cdn.alpinelinux.org/alpine/v3.21/community/

# Workaround https://github.com/confluentinc/librdkafka/issues/4897
RUN cp /usr/lib/librdkafka.so.1 /app/alpine-librdkafka.so
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "word-count.dll"]
