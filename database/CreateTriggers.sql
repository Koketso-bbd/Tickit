CREATE TRIGGER [trg_NotifyUserOnTaskAssignment] ON [tickit].[Tasks]
AFTER INSERT
AS
BEGIN
    INSERT INTO [tickit].[Notifications] 
		(UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
    SELECT i.AssigneeID, i.ProjectID, i.ID, 
           (SELECT ID FROM NotificationTypes WHERE NotificationName = 'Task Assigned'),
           'You have been assigned a new task: ' + i.TaskName,
           0, GETDATE()
    FROM inserted i
    WHERE i.AssigneeID IS NOT NULL;
END;
GO

CREATE TRIGGER [trg_InsertStatusTrack] ON [tickit].[Tasks]
AFTER UPDATE
AS
BEGIN
	IF UPDATE(StatusID)
	BEGIN
		INSERT INTO [tickit].[StatusTrack]
			(StatusID, TaskID, UpdatedAt)
		SELECT StatusID, ID, GETDATE() FROM inserted
	END
END;
GO

CREATE TRIGGER [trg_PreventStatusDowngrade] ON [tickit].[Tasks]
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

CREATE TRIGGER [trg_PreventDuplicatUserInProject]ON [tickit].[UserProjects]
INSTEAD OF INSERT
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [tickit].[UserProjects] up JOIN inserted i
		ON up.ProjectID = i.ProjectID AND up.MemberID = i.MemberID)
	
	BEGIN
		RAISERROR('This user is in this project', 16, 1);
	END
	ELSE
	BEGIN
		INSERT INTO [tickit].[UserProjects]
			(ProjectID, MemberID, RoleID)
		SELECT ProjectID, MemberID, RoleID FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_PreventDuplicateUserOnTask] ON [tickit].[Tasks]
INSTEAD OF INSERT
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [tickit].[Tasks] t JOIN inserted i
		ON t.ID = i.ID AND t.AssigneeID = i.AssigneeID)

	BEGIN
		RAISERROR('This user is already assigned to this task.', 16, 1);
		RETURN;
	END
	ELSE
	BEGIN
		INSERT INTO [tickit].[Tasks]
			(AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)
		SELECT AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_NotifyUserOnProjectAdded] ON [tickit].[UserProjects]
AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [tickit].[Notifications]
		(UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
	SELECT
		i.MemberID, i.ProjectID, NULL, nt.ID, CONCAT('You have been added to Project ', i.ProjectID),
		0, GETDATE()
	FROM inserted i JOIN [tickit].[NotificationTypes] nt ON nt.NotificationName = 'Added to Project';
END;
GO

CREATE TRIGGER [trg_DueDateCantBeSetInThePast] ON [tickit].[Tasks]
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