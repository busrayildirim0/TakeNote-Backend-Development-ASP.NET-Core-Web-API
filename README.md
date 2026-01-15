# 📝 TakeNote - Collaborative Note & Task Management API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![Azure](https://img.shields.io/badge/azure-%230072C6.svg?style=for-the-badge&logo=microsoftazure&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)

**TakeNote** is a comprehensive backend solution designed to bridge the gap between personal note-taking and team collaboration. It allows users to manage private notes, create shared workspaces, assign tasks, and collaborate in real-time.

---

##  Key Features

###  Authentication & Security
* **JWT Authentication:** Secure stateless authentication using JSON Web Tokens.
* **ASP.NET Core Identity:** Robust user management and password hashing.
* **RBAC (Role-Based Access Control):** Granular permissions (Admin, Editor, Viewer) within workspaces.

###  Collaboration (Workspaces)
* **Shared Environments:** Create private or public workspaces for teams.
* **Member Management:** Invite users via email and assign specific roles.
* **Data Isolation:** Strict separation between "Personal Notes" and "Workspace Notes".

###  Note & Task Management
* **Rich Notes:** Create, edit, and tag notes.
* **Task Integration:** Convert actionable items into Tasks linked directly to notes.
* **Versioning:** (Optional) Track changes in note history.

###  Real-Time & DevOps
* **SignalR Integration:** Real-time notifications for task assignments and workspace invites.
* **Docker Support:** Fully containerized application (API + SQL Server) using Docker Compose.
* **Cloud Ready:** Deployed and tested on **Azure App Service** and **Azure SQL Database**.

---

##  Architecture

The project follows a **Layered Architecture** (N-Tier) combined with the **Repository Pattern** and **Unit of Work** to ensure separation of concerns and maintainability.

### System Overview
![System Architecture](<img width="3675" height="724" alt="umling1" src="https://github.com/user-attachments/assets/5d6a8c5b-3d2e-4474-97c8-e52e5a5e6e52" />
 ,<img width="2793" height="1224" alt="umling2" src="https://github.com/user-attachments/assets/68625c47-eca5-4387-b2db-83b2d6e78722" />
 )



### Database Design (ERD)
The database schema handles complex relationships between Users, Workspaces, and Notes.
![ER Diagram]
[Untitled-1.py](https://github.com/user-attachments/files/24653253/Untitled-1.py)df1=pd.read_csv("C:\Users\Hp\OneDrive\Masaüstü\geant4_data\lar_tpc_data_20251010_160609.csv")
print(f"Number of rows in df1: {len(df1)}")


---

## 🛠️ Technology Stack

| Category | Technology |
| :--- | :--- |
| **Framework** | .NET 8 (Core) |
| **Language** | C# |
| **Database** | SQL Server (Azure & Local) |
| **ORM** | Entity Framework Core (Code-First) |
| **Containerization** | Docker & Docker Compose |
| **Real-Time** | SignalR |
| **Documentation** | Swagger / OpenAPI |
| **Testing** | Postman |

---

## 🐳 Getting Started (Docker)

The easiest way to run the application is using Docker.

### Prerequisites
* [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.

### Installation Steps

1.  **Clone the repository**
    ```bash
    git clone [https://github.com/busrayildirim0/TakeNote-Backend-Development-ASP.NET-Core-Web-API.git]
    cd TakeNote-Backend
    ```

2.  **Configure Environment**
    * Navigate to the `TakeNote.API` folder.
    * Update `appsettings.json` (or use User Secrets) with your database credentials if not using the default Docker container settings.

3.  **Run with Docker Compose**
    Open your terminal in the project root and run:
    ```bash
    docker-compose up --build
    ```

4.  **Access the API**
    * The API will be available at: `http://localhost:8080`
    * **Swagger UI:** `http://localhost:8080/swagger`

---

##  API Documentation & Testing

### Swagger UI
We provide full API documentation via Swagger. You can test all endpoints (Auth, Notes, Workspaces) directly from the browser.

<img width="1393" height="870" alt="Ekran görüntüsü 2026-01-16 003532" src="https://github.com/user-attachments/assets/a7468e49-2282-41ba-99ba-cb7c39d25bfb" />


### Postman Collection
We have tested complex scenarios (e.g., Admin creating a workspace -> Inviting a Member -> Member adding a Task) using Postman.


<img width="1919" height="972" alt="Ekran görüntüsü 2026-01-16 003416" src="https://github.com/user-attachments/assets/4f101839-46cb-42bd-9b3e-9b50cd9827c7" />

---

## 👥 The Team

This project was developed as a Capstone Project by:

| Team Member | Role | GitHub |
| :--- | :--- | :--- |
| **Büşra Yıldırım** | Backend & DevOps Lead | [@busrayildirim0](https://github.com/busrayildirim0) |
| **Emine Azmaz** | Backend & Database | [@emineazmaz](https://github.com/emineazmaz) |

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
