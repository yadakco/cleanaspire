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

# Install OpenSSL
RUN apt-get update && apt-get install -y openssl

# Create the /https directory
RUN mkdir /https

# Generate the self-signed certificate using OpenSSL
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -out /https/aspnetapp.crt \
  -keyout /https/aspnetapp.key \
  -subj "/CN=localhost"

# Optionally, convert to PFX format (uncomment if you choose Option B)
# RUN openssl pkcs12 -export -out /https/aspnetapp.pfx -inkey /https/aspnetapp.key -in /https/aspnetapp.crt -passout pass:

# Expose ports
EXPOSE 80 443

# Set the environment variable for ASP.NET Core to use Production settings
ENV ASPNETCORE_ENVIRONMENT=Development

# Start the application
ENTRYPOINT ["dotnet", "CleanAspire.Api.dll"]
