﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["ControlPanel.UI/ControlPanel.UI.csproj", "ControlPanel.UI/"]
COPY ["ControlPanel.Core/ControlPanel.Core.csproj", "ControlPanel.Core/"]
COPY ["ControlPanel.Data/ControlPanel.Data.csproj", "ControlPanel.Data/"]
RUN dotnet restore "ControlPanel.UI/ControlPanel.UI.csproj"
COPY . .
WORKDIR "/src/ControlPanel.UI"
RUN dotnet build "ControlPanel.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ControlPanel.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ControlPanel.UI.dll"]
