FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["streamiz-app.csproj", "streamiz-app/"]
RUN dotnet restore "streamiz-app/streamiz-app.csproj"
COPY . streamiz-app/
WORKDIR "/src/streamiz-app"
RUN dotnet build "streamiz-app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "streamiz-app.csproj" -c Release -o /app/publish

FROM base AS final
ARG ROCKSDB_VERSION=v7.4.3
WORKDIR /app
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "streamiz-app.dll"]
