# Stage 1: Build the API Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["src/CleanAspire.Api/CleanAspire.Api.csproj", "src/CleanAspire.Api/"]
COPY ["src/CleanAspire.Application/CleanAspire.Application.csproj", "src/CleanAspire.Application/"]
COPY ["src/CleanAspire.Domain/CleanAspire.Domain.csproj", "src/CleanAspire.Domain/"]
COPY ["src/CleanAspire.Infrastructure/CleanAspire.Infrastructure.csproj", "src/CleanAspire.Infrastructure/"]
COPY ["src/CleanAspire.ServiceDefaults/CleanAspire.ServiceDefaults.csproj", "src/CleanAspire.ServiceDefaults/"]
COPY ["src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj", "src/Migrators/Migrators.MSSQL/"]
COPY ["src/Migrators/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "src/Migrators/Migrators.PostgreSQL/"]
COPY ["src/Migrators/Migrators.SQLite/Migrators.SQLite.csproj", "src/Migrators/Migrators.SQLite/"]
RUN dotnet restore "src/CleanAspire.Api/CleanAspire.Api.csproj"

# Copy the entire source code and build the application in Release mode
COPY . .
WORKDIR /src/src/CleanAspire.Api
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ARG ENVIRONMENT=Development
ARG ENVIRONMENT=Development

# Copy the build output from the previous stage
COPY --from=build /app/publish .

# Expose port 80 for HTTP traffic
EXPOSE 443

# Start the application
# Create a self-signed certificate
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p password

# Start the application with HTTPS support
ENTRYPOINT ["dotnet", "CleanAspire.Api.dll", "--environment", "$ENVIRONMENT", "--urls", "https://0.0.0.0:443"]
