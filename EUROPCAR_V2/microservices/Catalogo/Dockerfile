# Build desde la raíz del repositorio (Render: Root Directory vacío, Dockerfile path = esta ruta).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EUROPCAR_V2/Directory.Build.props", "EUROPCAR_V2/"]

COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Protos/"]
COPY ["EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.DataAccess/RedCar.Catalogo.DataAccess.csproj", "EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.DataAccess/"]
COPY ["EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.DataManagement/RedCar.Catalogo.DataManagement.csproj", "EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.DataManagement/"]
COPY ["EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Business/RedCar.Catalogo.Business.csproj", "EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Business/"]
COPY ["EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Api/RedCar.Catalogo.Api.csproj", "EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Api/"]

RUN dotnet restore "EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Api/RedCar.Catalogo.Api.csproj"

COPY ["EUROPCAR_V2/shared/", "EUROPCAR_V2/shared/"]
COPY ["EUROPCAR_V2/microservices/Catalogo/", "EUROPCAR_V2/microservices/Catalogo/"]

WORKDIR "/src/EUROPCAR_V2/microservices/Catalogo/RedCar.Catalogo.Api"
RUN dotnet publish "RedCar.Catalogo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Catalogo.Api.dll"]
