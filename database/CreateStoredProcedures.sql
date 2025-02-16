-- CREATING A PROJECT AND ASSIGNING A OWNER
CREATE PROCEDURE [sp_CreateProject]
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

        INSERT INTO Projects (ProjectName, ProjectDescription, OwnerID)
			VALUES (@ProjectName, @ProjectDescription, @UserID);
        SET @ProjectID = SCOPE_IDENTITY();

        INSERT INTO UserProjects (ProjectID, MemberID, RoleID)
			VALUES (@ProjectID, @UserID, 1);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Adding a User to a Project
CREATE PROCEDURE [sp_AddUserToProject]
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

        INSERT INTO UserProjects (ProjectID, MemberID, RoleID)
			VALUES (@ProjectID, @UserID, @RoleID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

--Procedure to create a task
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
GO

--Procedure for updating a task
CREATE PROCEDURE sp_UpdateTaskStatus
    @TaskID INT,
    @NewStatusID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Tasks WHERE ID = @TaskID)
        BEGIN
            RAISERROR('Task does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE Tasks
        SET StatusID = @NewStatusID
        WHERE ID = @TaskID;
        INSERT INTO StatusTrack (TaskID, StatusID, StartedAt)
        VALUES (@TaskID, @NewStatusID, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Procedure to mark a notification as being read
CREATE PROCEDURE sp_MarkNotificationAsRead
    @NotificationID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Notifications WHERE ID = @NotificationID)
        BEGIN
            RAISERROR('Notification does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE Notifications
        SET IsRead = 1
        WHERE ID = @NotificationID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Procedure to remove a user in a project
CREATE PROCEDURE sp_RemoveUserFromProject
    @UserID INT,
    @ProjectID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM UserProjects WHERE UserID = @UserID AND ProjectID = @ProjectID)
        BEGIN
            RAISERROR('User is not part of the project', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DELETE FROM UserProjects 
        WHERE UserID = @UserID AND ProjectID = @ProjectID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Procedure to retrieve a users tasks
CREATE PROCEDURE sp_GetUserTasks
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT T.ID, T.TaskName, T.TaskDescription, T.DueDate, T.Priority, T.ProjectID, S.StatusName
        FROM Tasks T
        INNER JOIN Status S ON T.StatusID = S.ID
        WHERE T.AssigneeID = @UserID;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
    END CATCH;
END;
GO

-- procedure to get unread notifications
CREATE PROCEDURE sp_GetUserNotifications
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT ID, Message, CreatedAt 
        FROM Notifications 
        WHERE UserID = @UserID AND IsRead = 0
        ORDER BY CreatedAt DESC;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
    END CATCH;
END;
GO

-- proceduer for creating a label
CREATE PROCEDURE sp_CreateLabel
    @LabelName VARCHAR(100),
    @ProjectID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @LabelName IS NULL OR LEN(@LabelName) = 0
        BEGIN
            RAISERROR('Label name cannot be empty', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM Projects WHERE ID = @ProjectID)
        BEGIN
            RAISERROR('Project does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO Labels (LabelName, ProjectID)
        VALUES (@LabelName, @ProjectID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- PROCEDURE FOR ASSIGNING LABEL TO TASK
CREATE PROCEDURE sp_AddLabelToTask
    @TaskID INT,
    @LabelID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Tasks WHERE ID = @TaskID)
        BEGIN
            RAISERROR('Task does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM Labels WHERE ID = @LabelID)
        BEGIN
            RAISERROR('Label does not exist', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO TaskLabels (TaskID, LabelID)
        VALUES (@TaskID, @LabelID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO