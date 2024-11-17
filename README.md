## CleanAspire - Modern .NET 9 Minimal API + Blazor WebAssembly PWA Template
[![.NET](https://github.com/neozhu/cleanaspire/actions/workflows/dotnet.yml/badge.svg)](https://github.com/neozhu/cleanaspire/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/neozhu/cleanaspire/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/neozhu/cleanaspire/actions/workflows/github-code-scanning/codeql)

![blazorclient](https://github.com/user-attachments/assets/013b167b-59fa-42d7-a2f7-ffec301c4e11)
### Overview

**CleanAspire** is a cutting-edge open-source project combining the latest .NET 9 Minimal API with Blazor WebAssembly, all organized with the principles of **Clean Architecture**. This template is built to accelerate the development of scalable and maintainable Progressive Web Applications (PWA) while keeping the codebase neat, modular, and future-proof.


### Features

- **.NET 9 Minimal API** for lightweight, high-performance backend services.
- **Blazor WebAssembly** for a modern, responsive client-side PWA.
- **Clean Architecture** principles to promote separation of concerns, maintainability, and scalability.
- **Minimal Boilerplate** for developers to jump-start their projects without repetitive setup.
- **RESTful APIs** that are efficient and easy to use.
- **Strong Typing** throughout the client and server to leverage the full power of C#.
- **Ready-to-Deploy** template for Azure, AWS, Docker, and other cloud platforms.

### Why CleanAspire?

- **Speed Up Development**: Start coding your features instead of setting up boilerplate code.
- **Modern and Performant**: Benefit from .NET 9's new features and the rich interactivity of Blazor WebAssembly as a PWA.
- **Modular and Testable**: Clean Architecture makes maintaining and testing your project a breeze.
- **Community-driven**: CleanAspire is open-source, encouraging contributions and collaboration from developers worldwide.

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

