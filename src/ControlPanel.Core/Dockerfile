# Используем базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем все необходимые проекты
COPY ./ControlPanel.Data ./ControlPanel.Data 
COPY ./ControlPanel.Core ./ControlPanel.Core

# Восстанавливаем зависимости
WORKDIR /app/ControlPanel.Core
RUN dotnet restore

# Публикуем проект
RUN dotnet publish -c Release -o out
