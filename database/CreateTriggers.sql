USE TickItDB;
GO

CREATE TRIGGER [trg_NotifyUserOnTaskAssignment] ON [dbo].[Tasks]
AFTER INSERT
AS
BEGIN
    INSERT INTO [dbo].[Notifications] 
		(UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
    SELECT i.AssigneeID, i.ProjectID, i.ID, 
           (SELECT ID FROM NotificationTypes WHERE NotificationName = 'Task Assigned'),
           'You have been assigned a new task: ' + i.TaskName,
           0, GETDATE()
    FROM inserted i
    WHERE i.AssigneeID IS NOT NULL;
END;
GO

CREATE TRIGGER [trg_InsertStatusTrack] ON [dbo].[Tasks]
AFTER UPDATE
AS
BEGIN
	IF UPDATE(StatusID)
	BEGIN
		INSERT INTO [dbo].[StatusTrack]
			(StatusID, TaskID, UpdatedAt)
		SELECT StatusID, ID, GETDATE() FROM inserted
	END
END;
GO

CREATE TRIGGER [trg_PreventStatusDowngrade] ON [dbo].[Tasks]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(StatusID)
    BEGIN
        IF EXISTS ( SELECT 1 FROM inserted i JOIN deleted d ON i.ID = d.ID
            WHERE i.StatusID < d.StatusID)

        BEGIN
			RAISERROR('Task status cannot be downgraded.', 16, 1);
            ROLLBACK TRANSACTION;
        END

    END
END;
GO

CREATE TRIGGER [trg_PreventDuplicatUserInProject]ON [dbo].[UserProjects]
INSTEAD OF INSERT
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [dbo].[UserProjects] up JOIN inserted i
		ON up.ProjectID = i.ProjectID AND up.MemberID = i.MemberID)
	
	BEGIN
		RAISERROR('This user is in this project', 16, 1);
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[UserProjects]
			(ProjectID, MemberID, RoleID)
		SELECT ProjectID, MemberID, RoleID FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_PreventDuplicateUserOnTask] ON [dbo].[Tasks]
INSTEAD OF INSERT
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [dbo].[Tasks] t JOIN inserted i
		ON t.ID = i.ID AND t.AssigneeID = i.AssigneeID)

	BEGIN
		RAISERROR('This user is already assigned to this task.', 16, 1);
		RETURN;
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[Tasks]
			(AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)
		SELECT AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_NotifyUserOnProjectAdded] ON [dbo].[UserProjects]
AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Notifications]
		(UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
	SELECT
		i.MemberID, i.ProjectID, NULL, nt.ID, CONCAT('You have been added to Project ', i.ProjectID),
		0, GETDATE()
	FROM inserted i JOIN [dbo].[NotificationTypes] nt ON nt.NotificationName = 'Added to Project';
END;
GO

CREATE TRIGGER [trg_DueDateCantBeSetInThePast] ON [dbo].[Tasks]
AFTER INSERT, UPDATE

AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE DueDate < CAST(GETDATE() AS DATE))

    BEGIN
        RAISERROR('Due date cannot be set in the past.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO