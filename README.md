# TickIt

**A task management system.**

**TickIt** is a project and task management tool made for teams who want to collaborate and assign tasks on a project. Teams are able to create projects, add users to these projects, create tasks and assign them to members in the project.

## Database Setup for TickIt  

The `database` folder contains SQL scripts required to set up the **TickIt** database successfully. Follow the steps below to create and configure the database.

---

### Setup Instructions  

#### Open SQL Server & Connect to Your Instance  
Ensure you have **Microsoft SQL Server** and **SQL Server Management Studio (SSMS)** installed.

#### Run the SQL Scripts in Order  

Run the following scripts **in sequence**:

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

The TickIt Database should be set up after this :). 