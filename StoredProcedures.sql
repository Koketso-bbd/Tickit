
--- ------------------ CREATING A PROJECT AND ASSIGNING A OWNER --------------------------

CREATE PROCEDURE sproc_CreateProject
    @ProjectName VARCHAR(100),
    @ProjectDescription NVARCHAR(1500),
    @UserID INT

AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ProjectID INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @ProjectName IS NULL OR LEN(@ProjectName) = 0
        BEGIN
            RAISERROR('Project name cannot be empty', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END


        INSERT INTO Projects (ProjectName, ProjectDescription, UserID, CreatedAt)
        VALUEs (@ProjectName, @ProjectDescription, @UserID, GETDATE());

        SET @ProjectID = SCOPE_IDENTITY();


        INSERT INTO UserProjects (ProjectID, UserID, RoleID, JoinedAt)
        VALUES (@ProjectID, @UserID, 1, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO
------------------------------------------------------------------------------------------




---------------------- Adding a User to a Project ---------------------------------------

CREATE PROCEDURE sp_AddUserToProject
    @UserID INT,
    @ProjectID INT,
    @RoleID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Projects WHERE ID = @ProjectID)
        BEGIN
            RAISERROR('Project does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO UserProjects (UserID, ProjectID, RoleID, JoinedAt)
        VALUES (@UserID, @ProjectID, @RoleID, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO;
--------------------------------------------------------------------------




--------------------Procedure to create a task--------------------------

CREATE PROCEDURE sp_CreateTask
    @TaskName VARCHAR(255),
    @TaskDescription NVARCHAR(1000),
    @DueDate DATETIME,
    @Priority TINYINT,
    @ProjectID INT,
    @AssigneeID INT NULL,
    @StatusID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @TaskName IS NULL OR LEN(@TaskName) = 0
        BEGIN
            RAISERROR('Task name cannot be empty', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        IF NOT EXISTS (SELECT 1 FROM Projects WHERE ID = @ProjectID)
        BEGIN
            RAISERROR('Project does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO Tasks (TaskName, TaskDescription, DueDate, Priority, ProjectID, AssigneeID, StatusID, CreatedAt, ConfirmationDate)
        VALUES (@TaskName, @TaskDescription, @DueDate, @Priority, @ProjectID, @AssigneeID, @StatusID, GETDATE(), GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
-----------------------------------------------------------------------------------------------------