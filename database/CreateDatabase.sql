CREATE TABLE [tickit].[Users] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [GitHubID] VARCHAR(39) NOT NULL,

    CONSTRAINT [PK_Users] PRIMARY KEY ([ID]),
    CONSTRAINT [UQ_GitHubID] UNIQUE ([GitHubID])
);
GO

CREATE TABLE [tickit].[Projects] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [ProjectName] VARCHAR(100) NOT NULL,
    [ProjectDescription] NVARCHAR(1500) NULL,
    [OwnerID] INT NOT NULL,

    CONSTRAINT [PK_Projects] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Project_Users] FOREIGN KEY ([OwnerID]) REFERENCES [tickit].[Users]([ID])
);
GO

CREATE TABLE [tickit].[Roles] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [RoleName] VARCHAR(30) NOT NULL,

    CONSTRAINT [PK_Roles] PRIMARY KEY ([ID]),
	CONSTRAINT [UQ_RoleName] UNIQUE ([RoleName])
);
GO

CREATE TABLE [tickit].[UserProjects] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [ProjectID] INT NOT NULL,
    [MemberID] INT NOT NULL,
    [RoleID] INT NOT NULL,

    CONSTRAINT [PK_UserProjects] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_UserProjects_Project] FOREIGN KEY ([ProjectID]) REFERENCES [tickit].[Projects]([ID]),
    CONSTRAINT [FK_UserProjects_Users] FOREIGN KEY ([MemberID]) REFERENCES [tickit].[Users]([ID]),
    CONSTRAINT [FK_UserProjects_Roles] FOREIGN KEY ([RoleID]) REFERENCES [tickit].[Roles]([ID])
);
GO

CREATE TABLE [tickit].[Labels] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [LabelName] VARCHAR(30) NOT NULL,
    
    CONSTRAINT [PK_Labels] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [tickit].[ProjectLabels] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [ProjectID] INT NOT NULL,
    [LabelID] INT NOT NULL,

    CONSTRAINT [PK_ProjectLabels] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_ProjectLabels_Project] FOREIGN KEY ([ProjectID]) REFERENCES [tickit].[Projects]([ID]),
    CONSTRAINT [FK_ProjectLabels_Label] FOREIGN KEY ([LabelID]) REFERENCES [tickit].[Labels]([ID]),
	CONSTRAINT [UQ_ProjectLabel] UNIQUE (ProjectID, LabelID)
);
GO

CREATE TABLE [tickit].[Priority] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [PriorityLevel] VARCHAR(40) NOT NULL,

    CONSTRAINT [PK_Priority] PRIMARY KEY ([ID]),
	CONSTRAINT [UQ_PriorityLevel] UNIQUE ([PriorityLevel])
);
GO

CREATE TABLE [tickit].[Status] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [StatusName] VARCHAR(30) NOT NULL,

    CONSTRAINT [PK_Status] PRIMARY KEY ([ID]),
	CONSTRAINT [UQ_StatusName] UNIQUE ([StatusName])
);
GO

CREATE TABLE [tickit].[Tasks] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [AssigneeID] INT NOT NULL,
    [TaskName] VARCHAR(255) NOT NULL,
    [TaskDescription] NVARCHAR(1000) NULL,
    [DueDate] DATETIME NOT NULL,
    [PriorityID] INT NOT NULL,
    [ProjectID] INT NOT NULL,
    [StatusID] INT NOT NULL,

    CONSTRAINT [PK_Tasks] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Tasks_Users] FOREIGN KEY ([AssigneeID]) REFERENCES [tickit].[Users]([ID]),
    CONSTRAINT [FK_Tasks_Priority] FOREIGN KEY ([PriorityID]) REFERENCES [tickit].[Priority]([ID]),
    CONSTRAINT [FK_Tasks_Project] FOREIGN KEY ([ProjectID]) REFERENCES [tickit].[Projects]([ID]),
    CONSTRAINT [FK_Tasks_Status] FOREIGN KEY ([StatusID]) REFERENCES [tickit].[Status]([ID])
);
GO

CREATE TABLE [tickit].[TaskLabels] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [TaskID] INT NOT NULL,
    [ProjectLabelID] INT NOT NULL,

    CONSTRAINT [PK_TaskLabels] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_TaskLabels_Tasks] FOREIGN KEY ([TaskID]) REFERENCES [tickit].[Tasks]([ID]),
    CONSTRAINT [FK_TaskLabels_Labels] FOREIGN KEY ([ProjectLabelID]) REFERENCES [tickit].[ProjectLabels]([ID]),
	CONSTRAINT [UQ_TaskLabels] UNIQUE (TaskID, ProjectLabelID)
);
GO

CREATE TABLE [tickit].[NotificationTypes] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [NotificationName] VARCHAR(50) NOT NULL,

    CONSTRAINT [PK_NotificationTypes] PRIMARY KEY ([ID]),
	CONSTRAINT [UQ_NotificationName] UNIQUE ([NotificationName])
);
GO

CREATE TABLE [tickit].[Notifications] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [UserID] INT NOT NULL,
    [ProjectID] INT NOT NULL,
    [TaskID] INT NULL,
    [NotificationTypeID] INT NOT NULL,
    [Message] NVARCHAR(255) NOT NULL,
    [IsRead] BIT NOT NULL,
    [CreatedAt] DATETIME NOT NULL,

    CONSTRAINT [PK_Notifications] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserID]) REFERENCES [tickit].[Users]([ID]),
    CONSTRAINT [FK_Notifications_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [tickit].[Projects]([ID]),
    CONSTRAINT [FK_Notifications_Tasks] FOREIGN KEY ([TaskID]) REFERENCES [tickit].[Tasks]([ID]),
    CONSTRAINT [FK_Notifications_NotificationTypes] FOREIGN KEY ([NotificationTypeID]) REFERENCES [tickit].[NotificationTypes]([ID])
);
GO

CREATE TABLE [tickit].[StatusTrack] (
    [ID] INT IDENTITY(1, 1) NOT NULL,
    [StatusID] INT NOT NULL,
    [TaskID] INT NOT NULL,
    [UpdatedAt] DATETIME NULL

    CONSTRAINT [PK_StatusTrack] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_StatusTrack_Status] FOREIGN KEY ([StatusID]) REFERENCES [tickit].[Status]([ID]),
    CONSTRAINT [FK_StatusTrack_Task] FOREIGN KEY ([TaskID]) REFERENCES [tickit].[Tasks]([ID]),
);
GO