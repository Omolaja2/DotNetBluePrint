# 🏗️ Blueprint Studio
### *Sophisticated .NET Scaffolding Engine for Architectural Excellence*

[![Platform](https://img.shields.io/badge/.NET-9.0-blueviolet)](https://dotnet.microsoft.com/download)
[![Database](https://img.shields.io/badge/Database-MySQL%20%7C%20PostgreSQL%20%7C%20SQLite%20%7C%20SQLServer-blue)](https://aiven.io/)
[![Deployment](https://img.shields.io/badge/Deploy-Render-00b3b0)](https://render.com)

**Blueprint Studio** is a premium, high-performance project generator designed for architects and developers who demand more than a default template. It leverages a sophisticated engine to "forge" production-ready .NET solutions with your choice of architecture, database, and runtime version in seconds.

---

## 🌟 Key Features

- **Architectural Excellence**: Forge projects using elite design patterns:
  - **Clean Architecture** (Domain-centric)
  - **Hexagonal Architecture** (Ports & Adapters)
  - **Microservices** (Distributed Systems)
  - **CQRS** (Command Query Responsibility Segregation)
  - **Layered / MVC / MVVM** (Industry Standards)
- **Multi-Cloud Database Support**: Native integration for SQL Server, PostgreSQL, MySQL, and SQLite.
- **Instant Forge**: High-speed project generation that bundles your entire solution into a downloadable `.zip` package.
- **Premium User Experience**: Specialized glassmorphic UI, smooth scroll-reveal animations, and high-fidelity design aesthetics.
- **Automated Communication**: Integrated `EmailService` that triggers professional HTML welcome messages to new architects.
- **Studio Manager**: A built-in management dashboard for monitoring registered users and system activity.

---

## 🛠️ Technology Stack

- **Core**: .NET 9.0 (ASP.NET Core MVC)
- **Data**: Entity Framework Core + Pomelo (MySQL)
- **Database**: Aiven (Managed MySQL)
- **Frontend**: Vanilla CSS (Custom Design System), Bootstrap Icons, Google Fonts (Inter)
- **Deployment**: Docker + Render (Web Services)
- **Auth**: Microsoft Identity (Password Hashing) + Cookie Authentication

---

## 🚀 Getting Started

### Local Development

1. **Clone the Forge**:
   ```bash
   git clone https://github.com/Omolaja2/DotNetBluePrint.git
   cd DotNetBluePrint
   ```

2. **Configure Database**:
   Update `appsettings.json` with your MySQL connection string or use SQLite for local testing.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost;user=root;password=your_pass;database=blueprint_db"
   }
   ```

3. **Ignite the Engine**:
   ```bash
   dotnet run
   ```
   *Access the studio at `http://localhost:5078`*

---

## 🐋 Production & Deployment

The studio is optimized for **Render** using a custom Docker configuration that includes the full .NET SDK for runtime project generation.

### Environment Variables
For production deployment, ensure the following variables are set:
- `ConnectionStrings__DefaultConnection`: Your Aiven MySQL string.
- `ASPNETCORE_ENVIRONMENT`: `Production` (or `Development` for debugging).
- `DOTNET_USE_POLLING_FILE_WATCHER`: `true` (to avoid inotify limits).

---

## 📂 Project Structure

- **/Controllers**: High-speed logic handling (Generator, Auth, Management).
- **/Services**: The "Forge Core" powered by `ProjectGeneratorService` and `ZipService`.
- **/Views**: Premium Razor templates with custom animation controllers.
- **/wwwroot/css**: The `generator.css` design system.
- **/Data**: `AppDbContext` for local and cloud persistence.

---

## 🛡️ Studio Management

Authorized administrators can access the **Studio Manager** at `/Management/Users` to view the register of elite architects currently utilizing the forge.

---

### *Forged with precision. Designed for excellence.*
**© 2026 Blueprint Studio | Omolaja2**
