# Contexto Docker = carpeta EUROPCAR_V2 (Render: Root Directory "EUROPCAR_V2", Dockerfile "microservices/Clientes/Dockerfile").
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "shared/RedCar.Shared.Contracts/"]
COPY ["shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "shared/RedCar.Shared.Auth/"]
COPY ["shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "shared/RedCar.Shared.Protos/"]
COPY ["shared/RedCar.Shared.Events/RedCar.Shared.Events.csproj", "shared/RedCar.Shared.Events/"]
COPY ["shared/RedCar.Shared.Messaging/RedCar.Shared.Messaging.csproj", "shared/RedCar.Shared.Messaging/"]
COPY ["microservices/Clientes/RedCar.Clientes.DataAccess/RedCar.Clientes.DataAccess.csproj", "microservices/Clientes/RedCar.Clientes.DataAccess/"]
COPY ["microservices/Clientes/RedCar.Clientes.DataManagement/RedCar.Clientes.DataManagement.csproj", "microservices/Clientes/RedCar.Clientes.DataManagement/"]
COPY ["microservices/Clientes/RedCar.Clientes.Business/RedCar.Clientes.Business.csproj", "microservices/Clientes/RedCar.Clientes.Business/"]
COPY ["microservices/Clientes/RedCar.Clientes.Api/RedCar.Clientes.Api.csproj", "microservices/Clientes/RedCar.Clientes.Api/"]

RUN dotnet restore "microservices/Clientes/RedCar.Clientes.Api/RedCar.Clientes.Api.csproj"

COPY ["shared/", "shared/"]
COPY ["microservices/Clientes/", "microservices/Clientes/"]

WORKDIR "/src/microservices/Clientes/RedCar.Clientes.Api"
RUN dotnet publish "RedCar.Clientes.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Clientes.Api.dll"]
