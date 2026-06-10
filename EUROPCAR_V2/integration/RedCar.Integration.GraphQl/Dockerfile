# Render: Root Directory = EUROPCAR_V2, Dockerfile path = integration/RedCar.Integration.GraphQl/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]

COPY ["shared/RedCar.Shared.Auth/RedCar.Shared.Auth.csproj", "shared/RedCar.Shared.Auth/"]
COPY ["shared/RedCar.Shared.Contracts/RedCar.Shared.Contracts.csproj", "shared/RedCar.Shared.Contracts/"]
COPY ["shared/RedCar.Shared.Messaging/RedCar.Shared.Messaging.csproj", "shared/RedCar.Shared.Messaging/"]
COPY ["integration/RedCar.Integration.GraphQl/RedCar.Integration.GraphQl.csproj", "integration/RedCar.Integration.GraphQl/"]

RUN dotnet restore "integration/RedCar.Integration.GraphQl/RedCar.Integration.GraphQl.csproj"

COPY ["shared/", "shared/"]
COPY ["integration/RedCar.Integration.GraphQl/", "integration/RedCar.Integration.GraphQl/"]

WORKDIR "/src/integration/RedCar.Integration.GraphQl"
RUN dotnet publish "RedCar.Integration.GraphQl.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RedCar.Integration.GraphQl.dll"]
