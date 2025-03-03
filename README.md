# TickIt API

TickIt API is a web application built with ASP.NET Core and Entity Framework Core. It provides a RESTful API for managing tickets and related data.

## Table of Contents

- [Getting Started](#getting-started)
- [Prerequisites](#prerequisites)
- [Database Setup](#database)
- [WebAPI Setup](#api)
- [Configuration](#configuration)
- [Usage](#usage)


## Getting Started

These instructions will help you set up and run the project on your local machine for development and testing purposes.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

## Database

Run the following scripts in order

1. Create The Database
    ```bash
    CreateDatabase.sql
    ```

2. Create The Triggers
    ```bash
    CreateTriggers.sql
    ```

3. Create The Stored Procedures
    ```bash
    CreateStoredProcedures.sql
    ```
    
4. Create The Functions
    ```bash
    CreateFunctions.sql
    ```

5. Create The Views
    ```bash
    CreateViews.sql
    ```

Your database should be set up successfully now.

## API

1. Navigate into the API folder
    ```bash
    cd api
    ```

2. Restore the dependancies
    ```bash
    dotnet restore
    ```

*If for any reason the dependencies failed to install you can install them manually using the following commands*
```bash
    dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.2
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.2
    dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.2
    dotnet add package Swashbuckle.AspNetCore --version 6.6.2
    dotnet add package Swashbuckle.AspNetCore.Annotations --version 7.2.0
    dotnet add package xunit --version 2.9.3
    dotnet add package xunit.runner.visualstudio --version 3.0.2
    dotnet add package Moq --version 4.20.72
    dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.2
    dotnet add package Newtonsoft.Json --version 13.0.3
    dotnet add package Microsoft.TestPlatform --version 17.13.0
    dotnet add package Microsoft.NET.Test.Sdk --version 17.13.0
```

3. Add Secrets

    * Create `secrets.json` inside of `Properties` directory.
    * Include the following in your secrets file:
    ```
    {
        "DBSERVER": <database server>,
        "DBNAME": <database name>,
        "DBUSERID": <database user ID>,
        "DBPASSWORD": <database password>
    }
    ```

4. Build the project
    ```bash
    dotnet build
    ```

## Usage

1. Run the application
    ```bash
    dotnet run
    ```

2. The API will be available at `http://localhost:5213/` and `https://localhost:7151` by default (unless you specify it differently on `launchSettings.json`).

3. Access the Swagger UI for the API documentation on `localhost:<port>/swagger

## Test

1. Navigate into the API folder `Tickit\api\api\`

2. Run tests
    ```bash
    dotnet test
    ```

