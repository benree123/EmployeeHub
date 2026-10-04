# EmployeeHub

EmployeeHub is a full-stack Employee Management System built with ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, and Bootstrap.

The application provides a clean and professional interface for managing employee records while demonstrating database integration, CRUD operations, search functionality, validation, and responsive web design.

## Features

- Add new employees
- View employee information
- Edit existing employee records
- Delete employee records
- Search employees by name, email, department, or position
- Store employee data using SQL Server
- Automatically seed fictional employee data for demonstration
- Form validation
- Responsive and professional user interface
- Employee information including:
  - First and last name
  - Email address
  - Department
  - Position
  - Salary
  - Hire date

## Technologies Used

- C#
- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server / LocalDB
- Razor Views
- HTML5
- CSS3
- Bootstrap
- Visual Studio 2022
- Git & GitHub

## Application Architecture

EmployeeHub follows the ASP.NET Core MVC architecture:

- **Models** — Employee data models and database context
- **Views** — Razor-based user interface
- **Controllers** — Application logic and request handling
- **Entity Framework Core** — Database access and persistence
- **SQL Server** — Employee data storage

## Database

The application uses Entity Framework Core with SQL Server LocalDB.

EF Core migrations are used to create and maintain the database structure.

The project also includes fictional employee seed data so the application can be demonstrated without using real employee information.

## Screenshots

Screenshots of the application interface will be added here.

<img width="955" height="472" alt="image" src="https://github.com/user-attachments/assets/5bb80d79-4ef2-491b-b90c-443f21343452" />
<img width="945" height="462" alt="image" src="https://github.com/user-attachments/assets/252edee1-2acf-4732-b919-bd343e454c0e" />
<img width="956" height="476" alt="image" src="https://github.com/user-attachments/assets/2d6072e0-3864-40a9-9a50-38598c9fb807" />
<img width="945" height="479" alt="image" src="https://github.com/user-attachments/assets/c3a57dfa-a439-4bab-87e4-44ffb02565aa" />
<img width="959" height="432" alt="image" src="https://github.com/user-attachments/assets/b36b1f3e-2ce3-46a4-88c7-af9039d5b683" />




## Getting Started

### Prerequisites

To run this project locally, you will need:

- Visual Studio 2022
- .NET 8 SDK
- SQL Server LocalDB

### Installation

1. Clone the repository.
2. Open the solution in Visual Studio 2022.
3. Restore NuGet packages.
4. Verify the database connection string in `appsettings.json`.
5. Run the application.

The application will apply the existing Entity Framework Core migrations and initialize the database with demonstration employee data when the employee table is empty.

## Purpose

This project was created as a software development portfolio project to demonstrate practical experience with:

- ASP.NET Core MVC development
- C# programming
- Relational databases
- Entity Framework Core
- CRUD operations
- MVC architecture
- Form validation
- Search functionality
- Responsive front-end development
- Git version control

## Future Improvements

Potential future improvements include:

- User authentication and authorization
- Employee dashboard and reporting
- Department management
- Pagination and advanced filtering
- REST API integration
- Cloud deployment

## Disclaimer

All employee names, email addresses, salaries, and other employee information used in this project are fictional and are included solely for demonstration purposes.

---

**EmployeeHub — Employee Management System**
