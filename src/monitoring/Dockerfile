FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /
COPY ["monitoring.csproj", "monitoring/"]
RUN dotnet restore "monitoring/monitoring.csproj"
COPY Program.cs /monitoring
WORKDIR "/monitoring"
RUN dotnet build "monitoring.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "monitoring.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "monitoring.dll"]
