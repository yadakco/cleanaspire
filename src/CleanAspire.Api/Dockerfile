# Stage 1: Build the API Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/CleanAspire.Api/CleanAspire.Api.csproj", "src/CleanAspire.Api/"]
COPY ["src/CleanAspire.Application/CleanAspire.Application.csproj", "src/CleanAspire.Application/"]
COPY ["src/CleanAspire.Domain/CleanAspire.Domain.csproj", "src/CleanAspire.Domain/"]
COPY ["src/CleanAspire.Infrastructure/CleanAspire.Infrastructure.csproj", "src/CleanAspire.Infrastructure/"]
COPY ["src/CleanAspire.ServiceDefaults/CleanAspire.ServiceDefaults.csproj", "src/CleanAspire.ServiceDefaults/"]
COPY ["src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj", "src/Migrators/Migrators.MSSQL/"]
COPY ["src/Migrators/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "src/Migrators/Migrators.PostgreSQL/"]
COPY ["src/Migrators/Migrators.SQLite/Migrators.SQLite.csproj", "src/Migrators/Migrators.SQLite/"]

RUN dotnet restore "src/CleanAspire.Api/CleanAspire.Api.csproj"

COPY . .
WORKDIR /src/src/CleanAspire.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the build output from the previous stage
COPY --from=build /app/publish .

# Expose ports 80 and 443 for HTTP and HTTPS traffic
EXPOSE 80 443

# Create a self-signed certificate
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p password

# Set the environment variable and start the application with HTTP and HTTPS support
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "CleanAspire.Api.dll", "--urls", "http://0.0.0.0:80;https://0.0.0.0:443"]
