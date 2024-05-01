# Базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем проект и восстанавливаем зависимости
COPY ./ControlPanel.Data ./ControlPanel.Data 
COPY ./ControlPanel.Core ./ControlPanel.Core
COPY ./ControlPanel.Bot ./ControlPanel.Bot

# Копируем остальной код и публикуем
COPY . ./
RUN dotnet publish -c Release -o out

# Базовый образ для выполнения
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Запуск бота
ENTRYPOINT ["dotnet", "ControlPanel.Bot.dll"]
