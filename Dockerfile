FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Teamup.csproj", "./"]
RUN dotnet restore "./Teamup.csproj"
COPY . .
RUN dotnet build "Teamup.csproj" -c Release -o /app/build
RUN dotnet publish "Teamup.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Cria o diretório de logs que a aplicação espera
RUN mkdir -p /var/www/teamup/logs && chown -R www-data:www-data /var/www/teamup/logs

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Teamup.dll"]
