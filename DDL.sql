USE TickItDB;
GO

CREATE TABLE [dbo].[Users](
	[ID] INT IDENTITY(1,1) NOT NULL,
	[GitHubID] VARCHAR(39) UNIQUE NOT NULL,

	CONSTRAINT [PK_Users] PRIMARY KEY ([ID] ASC)
);
GO


CREATE TABLE [dbo].[Projects](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [ProjectName] varchar(100) NOT NULL,
    [ProjectDescription] nvarchar(1500) NULL,
    [UserID] INT NOT NULL,
    [CreatedAt] DATETIME NOT NULL,

    CONSTRAINT [PK_Projects] PRIMARY KEY([ID] ASC),
    CONSTRAINT [FK_Project_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID])
);
GO

CREATE TABLE [dbo].[Roles](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [RoleName] VARCHAR(30) NOT NULL,

    CONSTRAINT [PK_Roles] PRIMARY KEY ([ID] ASC),
);
GO

CREATE TABLE [dbo].[UserProjects](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [ProjectID] INT NOT NULL,
    [UserID] INT NOT NULL,
    [RoleID] INT NOT NULL,
    [JoinedAt] DATETIME NOT NULL,

    CONSTRAINT [PK_UserProjects] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_UserProjects_Project] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects]([ID]),
    CONSTRAINT [FK_UserProjects_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([ID]),
    CONSTRAINT [FK_UserProjects_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles]([ID])
);
GO

CREATE TABLE [dbo].[NotificationTypes](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [Notification] VARCHAR(50) NOT NULL,

    CONSTRAINT [PK_NotificationType] PRIMARY KEY ([ID]),
    CONSTRAINT [UQ_Notification] UNIQUE ([Notification])
);
GO

CREATE TABLE [dbo].[Status](
    ID INT IDENTITY(1,1) NOT NULL,
    StatusName varchar(20) NOT NULL,

    CONSTRAINT [PK_Status] PRIMARY KEY ([ID]),

);
GO

CREATE TABLE [dbo].[Tasks](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [AssigneeID] INT NULL,
    [TaskName] VARCHAR(255) NOT NULL,
    [TaskDescription] NVARCHAR(1000) NULL,
    [DueDate] DATETIME NULL,
    [Priority] TINYINT NOT NULL CHECK (Priority BETWEEN 1 AND 4) DEFAULT(4),
    [ProjectID] INT NOT NULL,
    [StatusID] INT NOT NULL,
    [ConfirmationDate] DATETIME NOT NULL,
    [CompleitionDate] DATETIME NULL,
    [CreatedAt] DATETIME NOT NULL,

    CONSTRAINT [PK_Tasks] PRIMARY KEY ([ID] ASC),
    CONSTRAINT [FK_Tasks_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects](ID),
    CONSTRAINT [FK_Tasks_Users] FOREIGN KEY([AssigneeID]) REFERENCES [dbo].[Users](ID),
    CONSTRAINT [FK_Tasks_Status] FOREIGN KEY([StatusID]) REFERENCES [dbo].[Status]([ID]),
);
GO

CREATE TABLE[dbo].[Labels](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [LabelName] VARCHAR(50) NOT NULL,
    [ProjectID] INT NOT NULL,

    CONSTRAINT [PK_Labels] PRIMARY KEY ([ID] ASC),
    CONSTRAINT [FK_Labels_Project] FOREIGN KEY([ProjectID]) REFERENCES [dbo].[Projects]([ID]),
);
GO

CREATE TABLE[dbo].[TaskLabels](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [TaskID] INT NOT NULL,
    [LabelID] INT NOT NULL,

    CONSTRAINT [PK_TaskLabels] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_TaskLabels_Tasks] FOREIGN KEY([TaskID]) REFERENCES [dbo].[Tasks]([ID]),
    CONSTRAINT [FK_TaskLabels_Labels] FOREIGN KEY([LabelID]) REFERENCES [dbo].[Labels]([ID]),
);
GO

CREATE TABLE [dbo].[Notifications](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [UserID] INT NOT NULL,
    [ProjectID] INT NOT NULL,
    [TaskID] INT NULL,
    [NotificationTypeID] INT NOT NULL,
    [Message] NVARCHAR(255) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT(0),
    [CreatedAt] DATETIME NOT NULL

    CONSTRAINT [PK_Notifications] PRIMARY KEY ([ID] ASC),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY([UserID]) REFERENCES [dbo].[Users]([ID]),
    CONSTRAINT [FK_Notifications_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects]([ID]),
    CONSTRAINT [FK_Notifications_Tasks] FOREIGN KEY([TaskID]) REFERENCES [dbo].[Tasks]([ID]),
    CONSTRAINT [FK_Notifications_NotificationsType] FOREIGN KEY([NotificationTypeID]) REFERENCES [dbo].[NotificationTypes]([ID]),
);
GO


CREATE TABLE [dbo].[StatusTrack](
    [ID] INT IDENTITY(1,1) NOT NULL,
    [StatusID] INT NOT NULL,
    [TaskID] INT NOT NULL,
    [StartedAt] datetime ,

    CONSTRAINT [PK_StatusTrack] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_StatusTrack_Status] FOREIGN KEY([StatusID]) REFERENCES [dbo].[Status]([ID]),
    CONSTRAINT [FK_StatusTrack_Tasks] FOREIGN KEY([TaskID]) REFERENCES [dbo].[Tasks]([ID]),
);
GO

SELECT * FROM  [dbo].[StatusTrack]