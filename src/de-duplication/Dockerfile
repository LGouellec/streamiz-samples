FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["de-duplication.csproj", "de-duplication/"]
RUN dotnet restore "de-duplication/de-duplication.csproj"
COPY . de-duplication/
WORKDIR "/src/de-duplication"
RUN dotnet build "de-duplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "de-duplication.csproj" -c Release -o /app/publish

FROM base AS final
ARG ROCKSDB_VERSION=v7.4.3
WORKDIR /app
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "de-duplication.dll"]
