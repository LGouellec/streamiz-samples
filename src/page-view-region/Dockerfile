FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["page-view-region.csproj", "page-view-region/"]
RUN dotnet restore "page-view-region/page-view-region.csproj"
COPY . page-view-region/
WORKDIR "/src/page-view-region"
RUN dotnet build "page-view-region.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "page-view-region.csproj" -c Release -o /app/publish

FROM base AS final
ARG ROCKSDB_VERSION=v7.4.3
WORKDIR /app
RUN apk add --no-cache rocksdb libstdc++
RUN ln -s /usr/lib/librocksdb.so.7 /usr/lib/librocksdb.so
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "page-view-region.dll"]
