FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# TargetFramework net10.0 vive aqui; sin esto dotnet restore falla (NETSDK1013).
COPY ["Middleware.RedCar/Directory.Build.props", "Middleware.RedCar/"]
COPY ["EUROPCAR_V2/Directory.Build.props", "EUROPCAR_V2/"]

# Middleware
COPY ["Middleware.RedCar/Middleware.RedCar.sln", "Middleware.RedCar/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.Api/Middleware.RedCar.Api.csproj", "Middleware.RedCar/src/Middleware.RedCar.Api/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.Business/Middleware.RedCar.Business.csproj", "Middleware.RedCar/src/Middleware.RedCar.Business/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.DataManagement/Middleware.RedCar.DataManagement.csproj", "Middleware.RedCar/src/Middleware.RedCar.DataManagement/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.DataAccess/Middleware.RedCar.DataAccess.csproj", "Middleware.RedCar/src/Middleware.RedCar.DataAccess/"]

# Auth embebido: Seguridad.Business + shared (restaurar dependencias antes del COPY completo)
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]
COPY ["EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.DataAccess/RedCar.Seguridad.DataAccess.csproj", "EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.DataAccess/"]
COPY ["EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.DataManagement/RedCar.Seguridad.DataManagement.csproj", "EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.DataManagement/"]
COPY ["EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.Business/RedCar.Seguridad.Business.csproj", "EUROPCAR_V2/microservices/Seguridad/RedCar.Seguridad.Business/"]

RUN dotnet restore "Middleware.RedCar/src/Middleware.RedCar.Api/Middleware.RedCar.Api.csproj"

COPY ["Middleware.RedCar/", "Middleware.RedCar/"]
COPY ["EUROPCAR_V2/microservices/Seguridad/", "EUROPCAR_V2/microservices/Seguridad/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Auth/", "EUROPCAR_V2/shared/RedCar.Shared.Auth/"]
COPY ["EUROPCAR_V2/shared/RedCar.Shared.Contracts/", "EUROPCAR_V2/shared/RedCar.Shared.Contracts/"]

WORKDIR "/src/Middleware.RedCar/src/Middleware.RedCar.Api"

RUN dotnet publish "Middleware.RedCar.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Middleware.RedCar.Api.dll"]
