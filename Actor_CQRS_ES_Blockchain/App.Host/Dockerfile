#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["App.Host/App.Host.csproj", "App.Host/"]
COPY ["App.Actor/App.Actor.csproj", "App.Actor/"]
COPY ["App.IActor/App.IActor.csproj", "App.IActor/"]
COPY ["App.Framework/App.Framework.csproj", "App.Framework/"]
COPY ["App.Core/App.Core.csproj", "App.Core/"]
COPY ["App.Dapper/App.Dapper.csproj", "App.Dapper/"]
COPY ["App.Repository/App.Repository.csproj", "App.Repository/"]
COPY ["App.Handlers/App.Handlers.csproj", "App.Handlers/"]
RUN dotnet restore "App.Host/App.Host.csproj"
COPY . .
WORKDIR "/src/App.Host"
RUN dotnet build "App.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.Host.dll"]