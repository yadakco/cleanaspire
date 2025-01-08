# Stage 1: Build the Blazor Client Application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set ASPNETCORE_ENVIRONMENT to Standalone during build
ENV ASPNETCORE_ENVIRONMENT=Standalone

# Install Python for AOT compilation
RUN apt-get update && apt-get install -y python3 python3-pip && ln -s /usr/bin/python3 /usr/bin/python

# Copy the project files and restore dependencies
COPY ["src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj", "src/CleanAspire.ClientApp/"]
RUN dotnet restore "src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj"

# Install wasm-tools for AOT
RUN dotnet workload install wasm-tools --skip-manifest-update
RUN dotnet workload update

# Clean and build
RUN dotnet clean "src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj"

# Copy the entire source code and build the application in Release mode
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Configure and Run the Blazor WebAssembly Host
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Set ASPNETCORE_ENVIRONMENT to Standalone for the runtime environment
ENV ASPNETCORE_ENVIRONMENT=Standalone

# Copy the build output from the previous stage
COPY --from=build /app/publish .

# Install OpenSSL to create a self-signed certificate
RUN apt-get update && apt-get install -y openssl && \
    mkdir /https && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /https/aspnetcore.key -out /https/aspnetcore.crt \
    -subj "/CN=localhost"

# Configure Kestrel to use HTTPS with the self-signed certificate
ENV ASPNETCORE_URLS="http://+:80;https://+:443"

# Expose ports for HTTP and HTTPS
EXPOSE 80
EXPOSE 443

# Start the application using WebAssemblyHost
ENTRYPOINT ["dotnet", "CleanAspire.ClientApp.dll"]
