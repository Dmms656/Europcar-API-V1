# Contexto Docker = carpeta EUROPCAR_V2 (Render: Root Directory "EUROPCAR_V2", Dockerfile "microservices/Reservas/Dockerfile").
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "shared/RedCar.Shared.Contracts/"]
COPY ["shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "shared/RedCar.Shared.Auth/"]
COPY ["shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "shared/RedCar.Shared.Protos/"]
COPY ["microservices/Reservas/RedCar.Reservas.DataAccess/RedCar.Reservas.DataAccess.csproj", "microservices/Reservas/RedCar.Reservas.DataAccess/"]
COPY ["microservices/Reservas/RedCar.Reservas.DataManagement/RedCar.Reservas.DataManagement.csproj", "microservices/Reservas/RedCar.Reservas.DataManagement/"]
COPY ["microservices/Reservas/RedCar.Reservas.Business/RedCar.Reservas.Business.csproj", "microservices/Reservas/RedCar.Reservas.Business/"]
COPY ["microservices/Reservas/RedCar.Reservas.Api/RedCar.Reservas.Api.csproj", "microservices/Reservas/RedCar.Reservas.Api/"]

RUN dotnet restore "microservices/Reservas/RedCar.Reservas.Api/RedCar.Reservas.Api.csproj"

COPY ["shared/", "shared/"]
COPY ["microservices/Reservas/", "microservices/Reservas/"]

WORKDIR "/src/microservices/Reservas/RedCar.Reservas.Api"
RUN dotnet publish "RedCar.Reservas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Reservas.Api.dll"]
