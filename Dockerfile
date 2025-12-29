# Usar la imagen base de .NET 8.0 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos del proyecto
COPY ["CalificacionXPuntosWeb.csproj", "./"]
RUN dotnet restore "CalificacionXPuntosWeb.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "CalificacionXPuntosWeb.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "CalificacionXPuntosWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CalificacionXPuntosWeb.dll"]

