# Usar la imagen base de .NET 6
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Usar una imagen para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebAPIDockerUTN.csproj", "./"]
RUN dotnet restore "WebAPIDockerUTN.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WebAPIDockerUTN.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "WebAPIDockerUTN.csproj" -c Release -o /app/publish

# Configurar la aplicación para que utilice la imagen base
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPIDockerUTN.dll"]
