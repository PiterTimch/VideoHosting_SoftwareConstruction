# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY VideoHostingServer/VideoHostingServer.csproj VideoHostingServer/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
RUN dotnet restore VideoHostingServer/VideoHostingServer.csproj

COPY . .
WORKDIR /src/VideoHostingServer
RUN dotnet publish -c Release -o /app/publish

# ===== RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VideoHostingServer.dll"]
