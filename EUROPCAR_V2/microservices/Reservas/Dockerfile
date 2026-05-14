# Build desde la raíz del repositorio (Render: Root Directory vacío).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EUROPCAR_V2/Directory.Build.props", "EUROPCAR_V2/"]

COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Protos/"]
COPY ["EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.DataAccess/RedCar.Reservas.DataAccess.csproj", "EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.DataAccess/"]
COPY ["EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.DataManagement/RedCar.Reservas.DataManagement.csproj", "EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.DataManagement/"]
COPY ["EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Business/RedCar.Reservas.Business.csproj", "EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Business/"]
COPY ["EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Api/RedCar.Reservas.Api.csproj", "EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Api/"]

RUN dotnet restore "EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Api/RedCar.Reservas.Api.csproj"

COPY ["EUROPCAR_V2/shared/", "EUROPCAR_V2/shared/"]
COPY ["EUROPCAR_V2/microservices/Reservas/", "EUROPCAR_V2/microservices/Reservas/"]

WORKDIR "/src/EUROPCAR_V2/microservices/Reservas/RedCar.Reservas.Api"
RUN dotnet publish "RedCar.Reservas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Reservas.Api.dll"]
