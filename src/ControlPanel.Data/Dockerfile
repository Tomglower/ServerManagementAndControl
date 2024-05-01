# Базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем проект и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем и публикуем
COPY . ./
RUN dotnet publish -c Release -o out

# Базовый образ для выполнения
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Запуск проекта данных
ENTRYPOINT ["dotnet", "ControlPanel.Data.dll"]
