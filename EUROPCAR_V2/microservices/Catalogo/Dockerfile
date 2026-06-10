# Contexto Docker = carpeta EUROPCAR_V2 (Render: Root Directory "EUROPCAR_V2", Dockerfile "microservices/Catalogo/Dockerfile").
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "shared/RedCar.Shared.Contracts/"]
COPY ["shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "shared/RedCar.Shared.Auth/"]
COPY ["shared/RedCar.Shared.Protos/RedCar.Shared.Protos.csproj", "shared/RedCar.Shared.Protos/"]
COPY ["shared/RedCar.Shared.Events/RedCar.Shared.Events.csproj", "shared/RedCar.Shared.Events/"]
COPY ["shared/RedCar.Shared.Messaging/RedCar.Shared.Messaging.csproj", "shared/RedCar.Shared.Messaging/"]
COPY ["microservices/Catalogo/RedCar.Catalogo.DataAccess/RedCar.Catalogo.DataAccess.csproj", "microservices/Catalogo/RedCar.Catalogo.DataAccess/"]
COPY ["microservices/Catalogo/RedCar.Catalogo.DataManagement/RedCar.Catalogo.DataManagement.csproj", "microservices/Catalogo/RedCar.Catalogo.DataManagement/"]
COPY ["microservices/Catalogo/RedCar.Catalogo.Business/RedCar.Catalogo.Business.csproj", "microservices/Catalogo/RedCar.Catalogo.Business/"]
COPY ["microservices/Catalogo/RedCar.Catalogo.Api/RedCar.Catalogo.Api.csproj", "microservices/Catalogo/RedCar.Catalogo.Api/"]

RUN dotnet restore "microservices/Catalogo/RedCar.Catalogo.Api/RedCar.Catalogo.Api.csproj"

COPY ["shared/", "shared/"]
COPY ["microservices/Catalogo/", "microservices/Catalogo/"]

WORKDIR "/src/microservices/Catalogo/RedCar.Catalogo.Api"
RUN dotnet publish "RedCar.Catalogo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Catalogo.Api.dll"]
