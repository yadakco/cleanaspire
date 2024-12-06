## CleanAspire - .NET 9 Minimal API + Blazor WebAssembly PWA Template with Aspire Support 
[![.NET](https://github.com/neozhu/cleanaspire/actions/workflows/dotnet.yml/badge.svg)](https://github.com/neozhu/cleanaspire/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/neozhu/cleanaspire/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/neozhu/cleanaspire/actions/workflows/github-code-scanning/codeql)
[![Build and Push Docker Image](https://github.com/neozhu/cleanaspire/actions/workflows/docker.yml/badge.svg)](https://github.com/neozhu/cleanaspire/actions/workflows/docker.yml)

![blazorclient](./blazorclient.jpg)

### 🚀 Overview  

**CleanAspire** is a cutting-edge, open-source template built on **.NET 9**, designed to accelerate the development of **lightweight**, **fast**, and **simple** Blazor WebAssembly or Progressive Web Applications (PWA). It seamlessly integrates **Minimal APIs**, **Aspire**, and **Scalar** for modern API documentation.  

With a focus on **Clean Architecture** and **extreme code simplicity**, CleanAspire provides developers with the tools to create responsive and maintainable web applications with minimal effort. The template also supports **Microsoft.Kiota** to simplify API client generation, ensuring consistency and productivity in every project.  


### 🔑 Key Features  

1. **Built-in Aspire Support**  
   - Fully integrated with **Aspire** for efficient application hosting and configuration.  
   - Simplifies the setup process while providing a robust foundation for lightweight applications.  

2. **Fast and Minimal .NET 9 Minimal APIs**  
   - Uses the latest .NET 9 features to create high-performance and efficient APIs.  
   - Includes **Scalar** for modern and concise OpenAPI documentation, replacing traditional Swagger tools.  

3. **Designed for Simplicity and Speed**  
   - Adopts extreme code simplicity for rapid development without sacrificing functionality.  
   - Ideal for developers looking to build quick, responsive Blazor WebAssembly applications or PWAs.  

4. **Blazor WebAssembly and PWA Integration**  
   - Combines the power of Blazor WebAssembly for interactive and lightweight client-side UIs.  
   - PWA capabilities ensure offline support and a seamless native-like experience.  

5. **Streamlined API Client Integration**  
   - Utilizes **Microsoft.Kiota** to automatically generate strongly-typed API clients, reducing development overhead.  
   - Ensures consistent and error-free client-server communication.  

6. **Clean Architecture**  
   - Promotes modular, maintainable, and testable codebases through clear architectural layers.  

7. **Cloud-Ready with Docker**  
   - Preconfigured for Docker, enabling easy deployment to cloud platforms or local environments.  

8. **Integrated CI/CD Pipelines**  
   - Includes GitHub Actions workflows for automated building, testing, and deployment.  


### 🌟 Why Choose CleanAspire?  

- **Lightweight and Fast:** Designed to create high-performance, minimal Blazor WebAssembly or PWA projects.  
- **Effortless Development:** Extreme simplicity in code makes it easy to start quickly and scale effectively.  
- **Advanced API Integration:** Automate client-side API generation with Microsoft.Kiota for faster results.  
- **Future-Ready Architecture:** Leverages the latest .NET 9 capabilities and Aspire hosting for modern web applications.
- 

### OpenAPI documentation
- https://apiservice.blazorserver.com/scalar/v1


### Here is an example of a docker-compose.yml file for a local Docker deployment:

```yml
version: '3.8'
services:
  apiservice:
    image: blazordevlab/cleanaspire-api:0.0.41
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - AllowedHosts=*
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - ASPNETCORE_HTTP_PORTS=80
      - ASPNETCORE_HTTPS_PORTS=443
      - DatabaseSettings__DBProvider=sqlite
      - DatabaseSettings__ConnectionString=Data Source=CleanAspireDb.db
      - AllowedCorsOrigins=https://cleanaspire.blazorserver.com,https://localhost:7123
      - SendGrid__ApiKey=<your API key>
      - SendGrid__DefaultFromEmail=<your email>
    ports:
      - "8019:80"
      - "8018:443"


  webfrontend:
    image: blazordevlab/cleanaspire-clientapp:0.0.41
    ports:
      - "8016:80"
      - "8017:443"



```

### Quick Start

1. **Clone the Repo**:
   ```bash
   git clone https://github.com/neozhu/cleanaspire.git
   ```

2. **Navigate to the Project Folder**:
   ```bash
   cd CleanAspire
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```

4. **Access the Application**:
   Open your browser and go to `https://localhost:5001` to see the Blazor WebAssembly PWA in action.

### Architecture

CleanAspire is structured following the **Clean Architecture** approach, dividing the solution into distinct layers:

```
CleanAspire/
│
├── src/
│   ├── CleanAspire.Api/                # API Layer - .NET Minimal API
│   ├── CleanAspire.Application/        # Application Layer - Business Logic
│   ├── CleanAspire.WebAssembly/        # UI Layer - Blazor WebAssembly (PWA)
│   ├── CleanAspire.Infrastructure/     # Infrastructure Layer - Data Access, External Services
│   ├── CleanAspire.Domain/             # Domain Layer - Core Entities, including EF entities
│   ├── CleanAspire.AppHost/            # Hosting Layer - Application hosting and configuration
│   └── CleanAspire.ServiceDefaults/    # Service Defaults - Predefined configurations for services
│
├── tests/
│   ├── CleanAspire.Application.Tests/  # Unit Tests for Application Layer
│   ├── CleanAspire.Api.Tests/          # Integration Tests for API Layer
│   └── CleanAspire.WebAssembly.Tests/  # UI Tests for Blazor WebAssembly
│
├── README.md                           # Project README
├── LICENSE                             # License Information
└── CleanAspire.sln                     # Solution File for Visual Studio / VS Code
```

### Contributions

Contributions are welcome! If you want to add features, report bugs, or just suggest improvements, please feel free to submit issues or pull requests.

### License

This project is licensed under the [MIT License](LICENSE).

### Get Involved

- **Star the Repository**: If you like CleanAspire, please give it a star! 🌟
- **Follow the Project**: Stay updated with the latest developments by watching the repository.
- **Join Discussions**: Share your thoughts and ideas to make CleanAspire even better.

### Contact

Feel free to reach out if you have questions or feedback. Let's build something amazing together with **CleanAspire**!

