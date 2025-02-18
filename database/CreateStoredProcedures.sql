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
CREATE PROCEDURE [sp_CreateTask]
	@AssigneeID INT,
	@TaskName VARCHAR(255),
	@TaskDescription NVARCHAR(1000),
	@DueDate DATETIME,
	@PriorityID INT,
	@ProjectID INT,
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

        INSERT INTO Tasks (AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)
        VALUES (@AssigneeID, @TaskName, @TaskDescription, @DueDate, @PriorityID, @ProjectID, @StatusID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

--Procedure for updating a task
CREATE PROCEDURE [sp_UpdateTaskStatus]
    @NewStatusID INT,
    @TaskID INT

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
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Procedure to mark a notification as being read
CREATE PROCEDURE [sp_MarkNotificationAsRead]
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
CREATE PROCEDURE [sp_RemoveUserFromProject]
    @UserID INT,
    @ProjectID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM UserProjects WHERE MemberID = @UserID AND ProjectID = @ProjectID)
        BEGIN
            RAISERROR('User is not part of the project', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DELETE FROM UserProjects 
        WHERE MemberID = @UserID AND ProjectID = @ProjectID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- Procedure to retrieve a users tasks
CREATE PROCEDURE [sp_GetUserTasks]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT T.ID, T.TaskName, T.TaskDescription, T.DueDate, T.PriorityID, T.ProjectID, S.StatusName
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
CREATE PROCEDURE [sp_GetUnreadUserNotifications]
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

-- Procedure for creating a label
CREATE PROCEDURE [sp_CreateLabel]
    @LabelName VARCHAR(100)
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

        IF EXISTS (SELECT 1 FROM Labels WHERE LabelName = @LabelName)
        BEGIN
            RAISERROR('Label already exists', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO Labels (LabelName)
        VALUES (@LabelName);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

-- PROCEDURE FOR ASSIGNING LABEL TO PROJECT
CREATE PROCEDURE [sp_AddLabelToProject]
    @ProjectID INT,
    @LabelName VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LabelID INT;
    DECLARE @ProjectLabelID INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @LabelID = ID FROM Labels WHERE LabelName = @LabelName;

        IF @LabelID IS NULL
        BEGIN
            INSERT INTO Labels (LabelName) VALUES (@LabelName);
            SET @LabelID = SCOPE_IDENTITY();
        END

        SELECT @ProjectLabelID = ID FROM ProjectLabels 
        WHERE ProjectID = @ProjectID AND LabelID = @LabelID;

        IF @ProjectLabelID IS NULL
        BEGIN
            INSERT INTO ProjectLabels (ProjectID, LabelID)
            VALUES (@ProjectID, @LabelID);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO

CREATE PROCEDURE [sp_AddLabelToTask]
    @TaskID INT,
    @ProjectLabelID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM ProjectLabels WHERE ID = @ProjectLabelID)
        BEGIN
            RAISERROR('Invalid ProjectLabelID.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        INSERT INTO TaskLabels (TaskID, ProjectLabelID)
        VALUES (@TaskID, @ProjectLabelID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        PRINT ERROR_MESSAGE();
        ROLLBACK TRANSACTION;
    END CATCH;
END;
GO