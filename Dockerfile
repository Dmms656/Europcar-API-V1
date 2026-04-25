FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos
COPY ["Europcar.Rental.slnx", "./"]
COPY ["src/Europcar.Rental.Api/Europcar.Rental.Api.csproj", "src/Europcar.Rental.Api/"]
COPY ["src/Europcar.Rental.Business/Europcar.Rental.Business.csproj", "src/Europcar.Rental.Business/"]
COPY ["src/Europcar.Rental.DataManagement/Europcar.Rental.DataManagement.csproj", "src/Europcar.Rental.DataManagement/"]
COPY ["src/Europcar.Rental.DataAccess/Europcar.Rental.DataAccess.csproj", "src/Europcar.Rental.DataAccess/"]

# Restaurar dependencias
RUN dotnet restore "src/Europcar.Rental.Api/Europcar.Rental.Api.csproj"

# Copiar el resto del código
COPY . .
WORKDIR "/src/src/Europcar.Rental.Api"

# Construir y publicar
RUN dotnet build "Europcar.Rental.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Europcar.Rental.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen de producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto estándar que Render usa
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Europcar.Rental.Api.dll"]
