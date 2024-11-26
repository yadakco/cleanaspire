# Stage 1: Build the API Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["CleanAspire.Api.csproj", "./"]
COPY ["../CleanAspire.Application/CleanAspire.Application.csproj", "../CleanAspire.Application/"]
COPY ["../CleanAspire.Domain/CleanAspire.Domain.csproj", "../CleanAspire.Domain/"]
COPY ["../CleanAspire.Infrastructure/CleanAspire.Infrastructure.csproj", "../CleanAspire.Infrastructure/"]
COPY ["../CleanAspire.ServiceDefaults/CleanAspire.ServiceDefaults.csproj", "../CleanAspire.ServiceDefaults/"]
COPY ["../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj", "../Migrators/Migrators.MSSQL/"]
COPY ["../Migrators/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "../Migrators/Migrators.PostgreSQL/"]
COPY ["../Migrators/Migrators.SQLite/Migrators.SQLite.csproj", "../Migrators/Migrators.SQLite/"]
RUN dotnet restore "CleanAspire.Api.csproj"

# Copy the entire source code and build the application in Release mode
COPY .. .
WORKDIR /src
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
