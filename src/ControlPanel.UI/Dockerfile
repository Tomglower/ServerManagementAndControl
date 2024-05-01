# Базовый образ Angular
FROM node:16 AS build
WORKDIR /app

# Установка зависимостей
COPY package*.json ./
RUN npm install

# Сборка Angular-приложения
COPY . ./
RUN npm run build --prod

# Базовый образ .NET SDK для API
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /api

COPY ./ControlPanel.Data ./ControlPanel.Data 
COPY ./ControlPanel.Core ./ControlPanel.Core
COPY ./ControlPanel.UI ./ControlPanel.UI
RUN dotnet restore

# Копируем и публикуем
COPY . ./
RUN dotnet publish -c Release -o out

# Базовый образ для выполнения API
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /api
COPY --from=build /api/out/ . 
COPY --from=build /app/dist/ /wwwroot/  


# Запуск API и Angular
ENTRYPOINT ["dotnet", "ControlPanel.UI.dll"]
