FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props ./
COPY src/UserManagement.Domain/UserManagement.Domain.csproj src/UserManagement.Domain/
COPY src/UserManagement.Contracts/UserManagement.Contracts.csproj src/UserManagement.Contracts/
COPY src/UserManagement.Services/UserManagement.Services.csproj src/UserManagement.Services/
COPY src/UserManagement.Repository.Sql/UserManagement.Repository.Sql.csproj src/UserManagement.Repository.Sql/
COPY src/UserManagement.Messaging/UserManagement.Messaging.csproj src/UserManagement.Messaging/
COPY src/UserManagement.Blazor/UserManagement.Blazor.csproj src/UserManagement.Blazor/
COPY src/UserManagement.API/UserManagement.API.csproj src/UserManagement.API/
RUN dotnet restore src/UserManagement.API/UserManagement.API.csproj

COPY src/ src/
RUN dotnet publish src/UserManagement.API/UserManagement.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserManagement.API.dll"]
