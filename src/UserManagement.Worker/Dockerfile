FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props ./
COPY src/UserManagement.Domain/UserManagement.Domain.csproj src/UserManagement.Domain/
COPY src/UserManagement.Contracts/UserManagement.Contracts.csproj src/UserManagement.Contracts/
COPY src/UserManagement.Services/UserManagement.Services.csproj src/UserManagement.Services/
COPY src/UserManagement.Repository.Sql/UserManagement.Repository.Sql.csproj src/UserManagement.Repository.Sql/
COPY src/UserManagement.Messaging/UserManagement.Messaging.csproj src/UserManagement.Messaging/
COPY src/UserManagement.Worker/UserManagement.Worker.csproj src/UserManagement.Worker/
RUN dotnet restore src/UserManagement.Worker/UserManagement.Worker.csproj

COPY src/ src/
RUN dotnet publish src/UserManagement.Worker/UserManagement.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER app
ENTRYPOINT ["dotnet", "UserManagement.Worker.dll"]
