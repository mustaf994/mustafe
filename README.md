# Re-Exam Management System

A Web-Based System for Managing University Re-Examination Applications, Scheduling, Results, and Analytics.

> **Status:** All 11 development phases complete. See [Development Order](#development-order) below.

## Technology Stack

- ASP.NET Core 9 MVC / C#
- Entity Framework Core 9 (Code First) + Microsoft SQL Server
- ASP.NET Core Identity (custom `ApplicationUser` / `ApplicationRole`)
- Bootstrap 5, Bootstrap Icons, Chart.js
- FluentValidation
- QuestPDF (PDF export, Community license) and ClosedXML (Excel export)
- xUnit + EF Core InMemory (unit tests)

## Architecture

Clean Architecture, four projects plus a test project, all under `.NET 9`:

```
ReExamManagementSystem.sln
├── src/
│   ├── ReExamManagementSystem.Domain          Entities, Enums, repository/UoW interfaces. No framework dependencies.
│   ├── ReExamManagementSystem.Application      DTOs, ViewModels, service interfaces, service implementations, FluentValidation validators.
│   ├── ReExamManagementSystem.Infrastructure   EF Core DbContext, entity configurations, generic repository/UnitOfWork, ASP.NET Core Identity types, seed data.
│   └── ReExamManagementSystem.Web              Controllers, Razor Views, Areas, filters, middleware, wwwroot.
└── tests/
    └── ReExamManagementSystem.UnitTests
```

Each layer only depends on the layer(s) inside it (`Web → Application/Infrastructure → Domain`). `Program.cs` composes the app from `AddApplicationServices()` and `AddInfrastructureServices()` extension methods defined in each layer, so controllers never construct services or touch EF Core directly.

`ApplicationUser`/`ApplicationRole` (extending ASP.NET Core Identity's base types) live in `Infrastructure/Identity`, not `Domain`, because the Domain layer must not depend on Identity. Domain entities that belong to a user (e.g. `Student`) reference the Identity user only by its string `Id` — never through a navigation property — keeping Domain persistence-ignorant.

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full SQL Server). LocalDB connection string is the default in `appsettings.json`.

### Configure the database connection

`src/ReExamManagementSystem.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ReExamManagementSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

If you're on LocalDB instead of a full SQL Server instance, use `Server=(localdb)\\mssqllocaldb;...` instead.

Override this per-environment in `appsettings.Development.json` or via user secrets / environment variables — never commit real credentials.

### Build and run

```bash
dotnet restore
dotnet build
dotnet run --project src/ReExamManagementSystem.Web
```

### Database migrations

```bash
# create a new migration after changing an entity or Fluent API configuration
dotnet ef migrations add <Name> --project src/ReExamManagementSystem.Infrastructure --startup-project src/ReExamManagementSystem.Web

# apply pending migrations to the database in the connection string
dotnet ef database update --project src/ReExamManagementSystem.Infrastructure --startup-project src/ReExamManagementSystem.Web

# remove the most recently added (not yet applied) migration
dotnet ef migrations remove --project src/ReExamManagementSystem.Infrastructure --startup-project src/ReExamManagementSystem.Web
```

In practice you rarely need to run these by hand: `Program.cs` calls `context.Database.MigrateAsync()` on startup (via `DbInitializer.SeedAsync`), so pending migrations are applied — and the database seeded — automatically every time the app runs.

### Test login credentials

All seeded accounts share the password **`Passw0rd!123`**.

| Role                  | Email                              |
|-----------------------|-------------------------------------|
| Administrator         | admin@reexam.edu                   |
| Examination Officer   | officer@reexam.edu                 |
| Student               | kwame.boateng@student.reexam.edu   |
| Student               | ama.serwaa@student.reexam.edu      |
| Student               | kojo.mensah@student.reexam.edu     |
| Student               | efua.asante@student.reexam.edu     |
| Student               | yaw.owusu@student.reexam.edu       |
| Student               | student@reexam.edu                 |
| Student               | abena.asamoah@student.reexam.edu   |
| Student               | kofi.owusuansah@student.reexam.edu |
| Student               | adjoa.nyarko@student.reexam.edu    |
| Student               | kwabena.darko@student.reexam.edu   |

The database is created and seeded automatically the first time the app runs (`DbInitializer.SeedAsync`, called from `Program.cs`) — no manual migration step is required for a fresh clone once the connection string points at a reachable SQL Server.

### Student user role

The **Student** role has its own Area (`Areas/Student`), scoped to the signed-in student's own data — a student can never see or act on another student's records. It covers:

- **Dashboard** — a landing page summarizing the student's academic status at a glance (results, eligibility, pending applications).
- **Academic Profile** (`ProfileController`) — a self-service page showing department, program, level, enrollment status, and academic result history, distinct from the generic Identity account page.
- **Results** (`ResultController`) — a read-only portal that only ever shows results the officer has explicitly *published*; an unpublished result stays invisible even if it already exists in the database.
- **Re-Exam Applications** (`ApplicationController`) — view courses the student is eligible for a re-exam in (computed automatically from failing results), submit an application for one, and cancel a pending application. Duplicate applications for the same student/course/term are rejected.
- **Exam Schedule** (`ExamScheduleController`) — view the student's own upcoming exam schedule, and generate a printable/downloadable **Examination Slip** (PDF via QuestPDF) with course, room, time, instructions, and a reference number.

Every one of these actions is enforced server-side by role-based authorization policies (not just hidden in the UI), and audit-logged where it changes data (e.g. submitting or cancelling an application).

**Try it:** sign in as `student@reexam.edu` / `Passw0rd!123` to exercise all of the above as a real student — it has a passing result (CS102), a failing result (CS101) with the resulting re-exam eligibility, and one pending re-exam application already seeded, so the Dashboard and Applications pages show real data out of the box (the Results page stays empty until an officer publishes a re-exam result — see [Test login credentials](#test-login-credentials) above for the full list of seeded student accounts).

## Running Tests

```bash
dotnet test tests/ReExamManagementSystem.UnitTests
```

23 unit tests cover the business logic that matters most for correctness — each one runs against an isolated EF Core InMemory database, so no SQL Server connection or seeded data is required:

- **`GradeCalculationServiceTests`** — every configured grading band maps to the right grade/grade-point/pass-fail outcome; a mark outside all configured bands throws instead of silently producing a wrong grade.
- **`EligibilityServiceTests`** — recomputing eligibility from failing results is idempotent (running it twice never creates a duplicate record) and correctly ignores passing results.
- **`ReExamApplicationServiceTests`** — a student can only apply for a course that's open for re-exam; duplicate applications for the same student/course/term are rejected; approving/rejecting an application notifies the student; rejecting without a reason is refused.
- **`ExamScheduleServiceTests`** — room double-booking, invigilator double-booking, and invalid time ranges are all rejected; back-to-back (non-overlapping) schedules are allowed.
- **`ResultServiceTests`** — entering a re-exam mark computes the final mark/grade/status automatically; publishing an unverified result is refused; publishing a verified result succeeds and notifies the student.

Every phase above was also manually exercised end-to-end against a real SQL Server database during development (login/authorization boundaries per role, full CRUD cycles, the eligibility → application → approval → scheduling → attendance → result → publication pipeline, PDF/Excel export file validity, and the audit trail) — see the commit history / development notes for specifics. Two real defects were found and fixed this way: a global EF Core delete-behavior override that broke ASP.NET Core Identity's own cascade deletes, and a status-code error page that wasn't receiving its route parameter.

## Deployment Guide

1. **Configuration** — never ship real secrets in `appsettings.json`. Set the following via environment variables, an Azure App Service/Key Vault configuration, or `dotnet user-secrets` in development:
   - `ConnectionStrings__DefaultConnection` — production SQL Server connection string.
   - `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromEmail` — without these, password-reset and notification emails are logged instead of sent (safe for a demo, not for production).
2. **Publish**
   ```bash
   dotnet publish src/ReExamManagementSystem.Web -c Release -o ./publish
   ```
3. **Database** — run `dotnet ef database update` against the production connection string before first boot, or let the app apply migrations automatically on startup (current default) if that's acceptable for your deployment process.
4. **HTTPS** — the app calls `UseHttpsRedirection()` and `UseHsts()` outside Development; put it behind a reverse proxy (IIS, Nginx, or a cloud load balancer) that terminates TLS, or configure Kestrel with a certificate directly.
5. **Hosting** — the published output is a standard ASP.NET Core app: deploy it to IIS (with the ASP.NET Core Hosting Bundle installed), a Linux host behind Nginx with systemd, a container (add a `Dockerfile` targeting `mcr.microsoft.com/dotnet/aspnet:9.0`), or Azure App Service.
6. **First run** — the seeded administrator account (`admin@reexam.edu` / `Passw0rd!123`) exists in every environment the seeder runs against. **Change or remove it before exposing a real deployment to the internet.**

## Development Order

- [x] Phase 1 — Project Foundation (solution structure, Clean Architecture, EF Core + SQL Server wiring, Identity configuration, Bootstrap 5 / Bootstrap Icons / Chart.js assets)
- [x] Phase 2 — Database (20 entities, relationships/constraints/indexes, EF Core Fluent API configurations, DbContext, initial migration verified against a real SQL Server instance, seed data)
- [x] Phase 3 — Authentication (login/logout, forgot/reset password with SMTP-or-log email sending, change password, account lockout, role-based authorization policies, per-role Admin/Officer/Student Areas with a shared sidebar+topbar shell, audit logging of auth events) — verified end-to-end (login, cross-role access correctly blocked, audit rows written)
- [x] Phase 4 — Administration (live-data dashboard with Chart.js analytics; full CRUD — search/paginate/create/edit/delete via modal forms — for Faculties, Departments, Programs, Courses, Academic Years, Semesters; delete-guards against dependent records; audit-logged) — verified end-to-end against the real database
- [x] Phase 5 — Students (Student Management CRUD with automatic Identity login provisioning/deprovisioning through an `IUserAccountService` abstraction, cascading Department→Program and AcademicYear→Semester dropdowns, Student Profile + Academic Result History detail page) — verified end-to-end including a provisioned student successfully signing in with its generated temporary password
- [x] Phase 6 — Re-Exam (automatic eligibility computation from failing results, Re-Exam Subject management, student application submission/cancellation with duplicate prevention, officer approval/rejection with required rejection reason, in-app notifications with a topbar bell) — verified end-to-end: eligibility → application → approval/rejection → student notification, with audit logging at every step
- [x] Phase 7 — Examination (Exam Room and Invigilator management; scheduling with automatic room/invigilator/student double-booking conflict detection and room-capacity enforcement; attendance recording per exam) — verified end-to-end including live room-conflict and invigilator-conflict rejection and a successful attendance save
- [x] Phase 8 — Results (configurable grade calculation from GradingRule bands, re-exam mark entry, verification, publication with student notification, student result portal that only ever shows published results) — verified end-to-end including confirming an unpublished result stays invisible to the student
- [x] Phase 9 — Reports & Analytics (Student List, Eligible Students, Applications, Results and Examination Timetable reports with real PDF export via QuestPDF and Excel export via ClosedXML, shared by Admin/Officer; an Analytics page with 6 more Chart.js charts covering application status, pass/fail rate, grade distribution, most-repeated courses, students by program, and applications by academic year) — verified end-to-end, including that the exported PDF/XLSX files are genuinely valid
- [x] Phase 10 — Supporting Features (audit log browsing with user/action/date filters, System Settings CRUD, centralized friendly error pages for both unhandled exceptions and HTTP status codes with no sensitive detail leakage; notifications and logging were already built in earlier phases) — verified end-to-end, fixing a real routing bug in the error-page wiring along the way
- [x] Phase 11 — Testing & Finalization (23 xUnit tests against EF Core InMemory covering grade calculation, eligibility idempotency, application duplicate-prevention, exam scheduling conflict detection, and the verify-before-publish result workflow; every phase above was also manually verified end-to-end against a real SQL Server database, catching two real bugs — see [Running Tests](#running-tests); final documentation and [Deployment Guide](#deployment-guide) above)
- [x] Post-Phase-11 gap fill — a spec audit turned up four explicit requirements the phases above hadn't covered yet, all now built and verified:
  - **User Management** (Admin can create/deactivate/delete Administrator and Examination Officer accounts, not just Students)
  - The **printable Examination Slip** (Student area, professional printable/downloadable-PDF slip with student/course/room/time details, instructions and a reference number, generated via a dedicated QuestPDF document layout)
  - **Grading Rule management** (Admin CRUD over the mark bands `GradeCalculationService` reads from, with overlap validation — completing the "no hard-coded grading thresholds" requirement)
  - A **Student self-service academic profile** (department/program/level/enrollment/status plus result history — distinct from the generic Identity account page)
