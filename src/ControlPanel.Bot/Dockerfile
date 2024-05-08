FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ControlPanel.Bot/ControlPanel.Bot.csproj", "ControlPanel.Bot/"]
COPY ["ControlPanel.Core/ControlPanel.Core.csproj", "ControlPanel.Core/"]
COPY ["ControlPanel.Data/ControlPanel.Data.csproj", "ControlPanel.Data/"]
RUN dotnet restore "ControlPanel.Bot/ControlPanel.Bot.csproj"
COPY . .
WORKDIR "/src/ControlPanel.Bot"
RUN dotnet build "ControlPanel.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ControlPanel.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControlPanel.Bot.dll"]
