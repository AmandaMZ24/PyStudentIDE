# PyStudentIDE

Academic platform for teaching introductory programming with an AI-proof Python IDE.

## Tech Stack
- **Desktop Client**: C# .NET 8 / WPF
- **Backend API**: ASP.NET Core 8 Web API
- **Database**: SQL Server (normalized to 4NF)
- **Crypto**: System.Security.Cryptography (RSA-2048, SHA-256)
- **Git**: LibGit2Sharp
- **Design Patterns**: Singleton, Strategy, Factory Method, Observer, Adapter

## Architecture Layers
1. **Presentation (UI)**: WPF client with integrated Python editor
2. **Application**: Auth, Assignment, Crypto validation, Test engine services
3. **Domain**: Core entities and business rules
4. **Infrastructure**: Data persistence, Git integration, crypto, logging
5. **API**: REST backend for server-side operations

## Prerequisites
- .NET 8 SDK
- SQL Server
- Python 3.x (interpreter for student code execution)
- Git
