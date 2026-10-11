# 🏃 RaceDay Event Management System

## PROG6212 - Programming 2B | Part 2: RESTful API Development

## Project Description
RaceDay is a RESTful API that powers the RaceDay event management system. It supports Organisers creating and managing events, and Participants enrolling and viewing results. Built with .NET 8, EF Core Code-First, SQL Server, and Session-based authentication with role enforcement.

## Technologies Used
- .NET 8 Web API
- Entity Framework Core 8 (Code First)
- SQL Server / SSMS
- Swashbuckle (Swagger)
- xUnit
- GitHub Actions

## Project Structure
```
RaceDay/
├── .github/workflows/ci.yml
├── docs/                        (Part 1 planning)
├── RaceDay.Api/                 (Web API + Models + Data + Controllers)
├── RaceDay.Contracts/           (DTOs)
└── RaceDay.Api.Tests/           (xUnit tests)
```

## Database Setup
1. Ensure SQL Server (Express) is running.
2. Update `appsettings.json` if needed.
3. Package Manager Console:
   ```
   Add-Migration InitialRaceDayDatabase
   Update-Database
   ```

## How to Run the API
1. Open `RaceDay.sln` in Visual Studio 2022.
2. Set `RaceDay.Api` as startup project.
3. Press **F5**. Browser opens at `https://localhost:{port}/swagger`.

## Authentication
Session-based. After login, `UserId` and `Role` are stored in session. Protected endpoints use the `[RequireRole]` filter.

## API Endpoints

| Method | Endpoint | Role |
|--------|----------|------|
| POST | /api/auth/register | Public |
| POST | /api/auth/login | Public |
| POST | /api/auth/logout | Any |
| GET | /api/profile/me | Any |
| PUT | /api/profile/me | Any |
| GET | /api/events | Any |
| GET | /api/events/{id} | Any |
| GET | /api/events/mine | Organiser |
| POST | /api/events | Organiser |
| PUT | /api/events/{id} | Organiser (owner) |
| DELETE | /api/events/{id} | Organiser (owner) |
| GET | /api/events/{eventId}/categories | Any |
| POST | /api/events/{eventId}/categories | Organiser (owner) |
| PUT | /api/categories/{id} | Organiser (owner) |
| DELETE | /api/categories/{id} | Organiser (owner) |
| POST | /api/enrolments | Participant |
| GET | /api/enrolments/mine | Participant |
| GET | /api/enrolments/event/{eventId} | Organiser (owner) |
| PUT | /api/enrolments/{id}/status | Organiser (owner) |
| POST | /api/results | Organiser (owner) |
| GET | /api/results/mine | Participant |
| GET | /api/results/event/{eventId} | Organiser (owner) |

## Swagger
Navigate to `/swagger` after starting the API.
![image alt](https://github.com/Mbonikazi/RaceDay/blob/d630c17f3dffcf8be87aa0172d003577fedadd82/Swagger%20image.jpeg)

![image alt](https://github.com/Mbonikazi/RaceDay/blob/64d2b593c81c2ccac4ea122caa87eb77a0d5f776/Swagger%20image2.jpeg)

![image alt](https://github.com/Mbonikazi/RaceDay/blob/2496ffe942d7622bfdb67a02e1a8ca3a1114f279/Screenshot_11-10-2026_15279_localhost.jpeg)

## 🧪 Unit Testing

The test project **`RaceDay.Api.Tests`** uses **xUnit** with **EF Core InMemory** for isolated, dependency-free tests.

### Test Coverage

| Test File | What It Tests |
|-----------|---------------|
| `PasswordHasherTests.cs` | Hashing, verification, case sensitivity |
| `AuthTests.cs` | Registration, login, duplicate email detection, role assignment |
| `RoleAndSessionTests.cs` | Role enforcement, session keys, access rules |
| `ModelRelationshipTests.cs` | Entity relationships (1-to-many, 1-to-1), ownership rules |

## ⚙️ CI/CD – GitHub Actions

The repository includes a CI workflow at `.github/workflows/ci.yml`. It runs automatically on every push and pull request to `master` or `main`.

### Workflow Steps

1. ✅ Checkout repository
2. ✅ Setup .NET 8 SDK
3. ✅ Restore NuGet packages
4. ✅ Build solution in Release mode
5. ✅ Run all unit tests

### Green Build

You can view the workflow history on the ![image alt]()


###  Screenshot
![image alt](https://github.com/Mbonikazi/RaceDay/blob/42aa11453932c92f21703b006ae296fa48b179c9/Screenshot%202026-10-09%20140747.png)

## Video Presentation
Unlisted YouTube: [https://youtu.be/akRExinh8xA](https://youtu.be/akRExinh8xA)
