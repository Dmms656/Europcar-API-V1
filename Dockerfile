FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Orquestador (API pública + compat /api/v1)
COPY ["Middleware.RedCar/Middleware.RedCar.sln", "Middleware.RedCar/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.Api/Middleware.RedCar.Api.csproj", "Middleware.RedCar/src/Middleware.RedCar.Api/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.Business/Middleware.RedCar.Business.csproj", "Middleware.RedCar/src/Middleware.RedCar.Business/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.DataManagement/Middleware.RedCar.DataManagement.csproj", "Middleware.RedCar/src/Middleware.RedCar.DataManagement/"]
COPY ["Middleware.RedCar/src/Middleware.RedCar.DataAccess/Middleware.RedCar.DataAccess.csproj", "Middleware.RedCar/src/Middleware.RedCar.DataAccess/"]

RUN dotnet restore "Middleware.RedCar/src/Middleware.RedCar.Api/Middleware.RedCar.Api.csproj"

COPY ["Middleware.RedCar/", "Middleware.RedCar/"]
WORKDIR "/src/Middleware.RedCar/src/Middleware.RedCar.Api"

RUN dotnet publish "Middleware.RedCar.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Middleware.RedCar.Api.dll"]
