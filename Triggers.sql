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

CREATE TRIGGER [trg_NotifiyOverdueTask] ON Tasks
AFTER UPDATE

AS
BEGIN
    IF UPDATE(DueDate)
    BEGIN
        INSERT INTO Notifications (UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt)
        SELECT 
            t.AssigneeID,
            t.ProjectID,
            t.ID,
            (SELECT ID FROM NotificationTypes WHERE Notification = 'Task Overdue'),
            CONCAT('Task "', t.TaskName, '" is overdue!'),
            0,
            GETDATE()
        FROM Tasks t
        JOIN inserted i ON t.ID = i.ID
        JOIN Status s ON t.StatusID = s.ID
        WHERE t.DueDate < GETDATE() AND s.StatusName != 'Completed';
    END
END;
GO

CREATE TRIGGER [trg_DeleteNotificationOfDeletedTask] ON [dbo].[Tasks]
AFTER DELETE

AS
BEGIN
	DELETE FROM [dbo].[Notifications] WHERE TaskID IN (SELECT ID FROM deleted);
END;
GO

--CREATE TRIGGER [trg_DefaultTaskStatus] ON [dbo].[Tasks]
--AFTER INSERT

--AS
--BEGIN
	--UPDATE [dbo].[Tasks]
	--SET StatusID = (SELECT ID FROM [dbo].[Status] WHERE StatusName = 'To Do')
	--WHERE StatusID IS NULL;
--END;
--GO