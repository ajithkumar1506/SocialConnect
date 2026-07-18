# SocialConnect

<div align="center">
  <h3>A modern, scalable Modular Monolith built with .NET 9</h3>
</div>

## 📖 Overview

SocialConnect is a backend application for a social media platform. Designed as a **Modular Monolith**, the architecture enforces strong boundaries between logical domains, ensuring that it is highly cohesive, loosely coupled, and perfectly positioned for a future transition to microservices if scaling requirements demand it.

This project was built to showcase clean architecture, domain-driven design (DDD), and modern backend engineering practices.

## 🏗️ Architecture Style

**Modular Monolith** with **Clean Architecture** principles.

The codebase is divided into four main layers:
1. **Domain**: Core business logic, entities, value objects, and domain events. (No external dependencies).
2. **Application**: Use cases, CQRS (Commands/Queries), MediatR handlers, and FluentValidation rules.
3. **Infrastructure**: External concerns like Entity Framework Core, SQL Server, Redis, RabbitMQ, and AWS S3.
4. **API**: The entry point, controllers, middleware, and dependency injection composition root.

This structure allows us to keep the business logic pure while keeping infrastructure details interchangeable. 

> [!TIP]
> The project has been designed with a clear migration path to Microservices. Each feature (Auth, Users, Posts, Messaging) is encapsulated, making it easy to split into independent services in the future. See `microservices_migration_plan.md` for details.

## 🚀 Tech Stack

*   **.NET 9** (C# 13)
*   **Entity Framework Core 9** (Code-First)
*   **SQL Server** (Relational Data)
*   **Redis** (Distributed Caching)
*   **RabbitMQ** (Event Bus / Messaging)
*   **Hangfire** (Background Jobs)
*   **MediatR** (CQRS)
*   **FluentValidation** (Validation Pipeline)
*   **Docker & Docker Compose** (Containerization & Local Dev)
*   **xUnit, Moq, FluentAssertions, Testcontainers** (Testing)

## 🛠️ Local Development Setup

To run this project locally, you will need **Docker** and the **.NET 9 SDK** installed.

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/yourusername/SocialConnect.git
    cd SocialConnect
    ```

2.  **Start Infrastructure Services:**
    The project relies on external services (SQL Server, Redis, RabbitMQ, Mailhog, S3Ninja). We use Docker Compose to spin these up easily.
    ```bash
    docker-compose up -d
    ```

3.  **Run Migrations:**
    Ensure the database is created and schema is applied.
    ```bash
    dotnet ef database update --project src/SocialConnect.Infrastructure --startup-project src/SocialConnect.API
    ```

4.  **Run the API:**
    ```bash
    cd src/SocialConnect.API
    dotnet run
    ```
    Alternatively, run it via your IDE (Visual Studio / Rider).

5.  **Access Swagger UI:**
    Navigate to `https://localhost:5001/swagger` to explore and test the API endpoints.

## 📚 Documentation

Detailed documentation can be found in the `docs/` folder:

*   [**Architecture & Data Flow**](docs/architecture.md): Details the CQRS request pipeline and domain event flow.
*   [**Database Schema**](docs/database-schema.md): Entity-Relationship Diagram (ERD) and table details.
*   [**API Endpoints**](docs/api-endpoints.md): Example requests and responses for key features.

## 🧪 Testing

The solution includes a comprehensive test suite targeting **>80% overall code coverage**:

*   **Domain Tests**: Verifies core business rules, entity validations, and value objects.
*   **Application Tests**: Mocks infrastructure using `Moq` to test use-case handlers, validators, and request pipelines.
*   **Infrastructure Tests**: Tests background jobs, audit logging interceptors, and infrastructure services.
*   **API / Integration Tests**: Verifies the HTTP request lifecycle and database persistence using a fast, isolated **SQLite in-memory** database provider.
*   **Smoke Tests**: Validates application container booting, dependency injection configurations, and basic database connectivity.

To run all tests and collect coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 Security

*   **Passwords**: Never stored in plain text. Hashed using BCrypt.
*   **Authentication**: JWT-based authentication with Access and Refresh tokens.
*   **Configuration**: Sensitive data like database credentials should be provided via `.env` files (for Docker) and `User Secrets` (for local .NET dev). Passwords are intentionally omitted from source control.

## 👨‍💻 Developer

**Ajith Kumar** 
Senior Backend Engineer Candidate showcasing modern .NET architecture and clean code practices.
