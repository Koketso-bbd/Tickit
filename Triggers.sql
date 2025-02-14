USE TickItDB;
GO

CREATE TRIGGER [trg_NotifyTaskAssignment] ON [dbo].[Tasks]
AFTER INSERT

AS
BEGIN
    INSERT INTO Notifications 
		(UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
    SELECT i.AssigneeID, i.ProjectID, i.ID, 
           (SELECT ID FROM NotificationTypes WHERE Notification = 'Task Assigned'),
           'You have been assigned a new task: ' + i.TaskName,
           0, GETDATE()
    FROM inserted i
    WHERE i.AssigneeID IS NOT NULL;
END;
GO

CREATE TRIGGER [trg_LogStatusChange] ON [dbo].[Tasks]
AFTER UPDATE

AS
BEGIN
	IF UPDATE(StatusID)
	BEGIN
		INSERT INTO StatusTrack (StatusID, TaskID, StartedAt)
		SELECT StatusID, ID, GETDATE() FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_InsertStatusTrack] ON [dbo].[Tasks]
AFTER UPDATE

AS
BEGIN
	IF UPDATE(StatusID)
	BEGIN
		INSERT INTO [dbo].[StatusTrack] (StatusID, TaskID, StartedAt)
		SELECT StatusID, ID, GETDATE() FROM inserted;
	END
END;
GO

CREATE TRIGGER [trg_UpdateCompletionDate] ON [dbo].[Tasks]
AFTER UPDATE

AS
BEGIN
    IF UPDATE(StatusID)
    BEGIN
        UPDATE t
        SET t.CompleitionDate = GETDATE()
        FROM [dbo].[Tasks] t
        INNER JOIN inserted i on t.ID = i.ID
        WHERE i.StatusID = 4  AND t.CompleitionDate IS NULL;
    END
END;
GO

CREATE TRIGGER trg_EnforceUniqueLabelWithinProject
ON [dbo].[Labels]
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN [dbo].[Labels] l
        ON i.ProjectID = l.ProjectID AND i.LabelName = l.LabelName
        WHERE i.ID <> l.ID
    )
    BEGIN
        RAISERROR('Label name must be unique within a project.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO