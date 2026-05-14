# Build desde la raíz del repositorio (Render: Root Directory vacío).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Protos/"]
COPY ["EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.DataAccess/RedCar.Clientes.DataAccess.csproj", "EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.DataAccess/"]
COPY ["EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.DataManagement/RedCar.Clientes.DataManagement.csproj", "EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.DataManagement/"]
COPY ["EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Business/RedCar.Clientes.Business.csproj", "EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Business/"]
COPY ["EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Api/RedCar.Clientes.Api.csproj", "EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Api/"]

RUN dotnet restore "EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Api/RedCar.Clientes.Api.csproj"

COPY ["EUROPCAR_V2/shared/", "EUROPCAR_V2/shared/"]
COPY ["EUROPCAR_V2/microservices/Clientes/", "EUROPCAR_V2/microservices/Clientes/"]

WORKDIR "/src/EUROPCAR_V2/microservices/Clientes/RedCar.Clientes.Api"
RUN dotnet publish "RedCar.Clientes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Clientes.Api.dll"]
