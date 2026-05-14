# Build desde la raíz del repositorio (Render: Root Directory vacío).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EUROPCAR_V2/Directory.Build.props", "EUROPCAR_V2/"]

COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Protos/"]
COPY ["EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.DataAccess/RedCar.Localizaciones.DataAccess.csproj", "EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.DataAccess/"]
COPY ["EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.DataManagement/RedCar.Localizaciones.DataManagement.csproj", "EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.DataManagement/"]
COPY ["EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Business/RedCar.Localizaciones.Business.csproj", "EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Business/"]
COPY ["EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Api/RedCar.Localizaciones.Api.csproj", "EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Api/"]

RUN dotnet restore "EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Api/RedCar.Localizaciones.Api.csproj"

COPY ["EUROPCAR_V2/shared/", "EUROPCAR_V2/shared/"]
COPY ["EUROPCAR_V2/microservices/Localizaciones/", "EUROPCAR_V2/microservices/Localizaciones/"]

WORKDIR "/src/EUROPCAR_V2/microservices/Localizaciones/RedCar.Localizaciones.Api"
RUN dotnet publish "RedCar.Localizaciones.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Localizaciones.Api.dll"]
