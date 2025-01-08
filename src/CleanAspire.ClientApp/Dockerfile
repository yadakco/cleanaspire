# Stage 1: Build the Blazor Client Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install Python for AOT compilation
RUN apt-get update && apt-get install -y python3 python3-pip && ln -s /usr/bin/python3 /usr/bin/python

# Copy the project files and restore dependencies
COPY ["src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj", "src/CleanAspire.ClientApp/"]
RUN dotnet workload install wasm-tools --skip-manifest-update
RUN dotnet restore "./src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj"
COPY . .
WORKDIR "/src/src/CleanAspire.ClientApp"
RUN dotnet build "./CleanAspire.ClientApp.csproj" -c $BUILD_CONFIGURATION -o /app/build


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanAspire.ClientApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# Stage 2: Configure and Run the Blazor WebAssembly Host
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install OpenSSL
RUN apt-get update && apt-get install -y openssl

# Generate a self-signed certificate
RUN mkdir -p /app/https && \
    openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
    -keyout /app/https/private.key -out /app/https/certificate.crt \
    -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost" && \
    openssl pkcs12 -export -out /app/https/aspnetapp.pfx \
    -inkey /app/https/private.key -in /app/https/certificate.crt \
    -password pass:CREDENTIAL_PLACEHOLDER


# Configure Kestrel to use HTTPS with the self-signed certificate
# Setup environment variables for the application to find the certificate
ENV ASPNETCORE_ENVIRONMENT=Standalone
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="CREDENTIAL_PLACEHOLDER"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/https/aspnetapp.pfx"

# Expose ports for HTTP and HTTPS
EXPOSE 80
EXPOSE 443

# Start the application using WebAssemblyHost
ENTRYPOINT ["dotnet", "CleanAspire.ClientApp.dll"]
