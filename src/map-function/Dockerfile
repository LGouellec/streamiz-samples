FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["map-function.csproj", "map-function/"]
RUN dotnet restore "map-function/map-function.csproj"
COPY ["Program.cs", "map-function/"]
WORKDIR "/src/map-function"
RUN dotnet build "map-function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "map-function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apk add librdkafka --repository=https://dl-cdn.alpinelinux.org/alpine/v3.21/community/

# Workaround https://github.com/confluentinc/librdkafka/issues/4897
RUN cp /usr/lib/librdkafka.so.1 /app/alpine-librdkafka.so
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "map-function.dll"]
