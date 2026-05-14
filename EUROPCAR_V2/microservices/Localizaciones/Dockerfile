# Contexto Docker = carpeta EUROPCAR_V2 (Render: Root Directory "EUROPCAR_V2", Dockerfile "microservices/Localizaciones/Dockerfile").
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "shared/RedCar.Shared.Contracts/"]
COPY ["shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "shared/RedCar.Shared.Auth/"]
COPY ["shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "shared/RedCar.Shared.Protos/"]
COPY ["microservices/Localizaciones/RedCar.Localizaciones.DataAccess/RedCar.Localizaciones.DataAccess.csproj", "microservices/Localizaciones/RedCar.Localizaciones.DataAccess/"]
COPY ["microservices/Localizaciones/RedCar.Localizaciones.DataManagement/RedCar.Localizaciones.DataManagement.csproj", "microservices/Localizaciones/RedCar.Localizaciones.DataManagement/"]
COPY ["microservices/Localizaciones/RedCar.Localizaciones.Business/RedCar.Localizaciones.Business.csproj", "microservices/Localizaciones/RedCar.Localizaciones.Business/"]
COPY ["microservices/Localizaciones/RedCar.Localizaciones.Api/RedCar.Localizaciones.Api.csproj", "microservices/Localizaciones/RedCar.Localizaciones.Api/"]

RUN dotnet restore "microservices/Localizaciones/RedCar.Localizaciones.Api/RedCar.Localizaciones.Api.csproj"

COPY ["shared/", "shared/"]
COPY ["microservices/Localizaciones/", "microservices/Localizaciones/"]

WORKDIR "/src/microservices/Localizaciones/RedCar.Localizaciones.Api"
RUN dotnet publish "RedCar.Localizaciones.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Localizaciones.Api.dll"]
