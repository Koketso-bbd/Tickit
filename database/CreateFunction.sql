CREATE FUNCTION dbo.fn_GetTaskCount(@ProjectID INT) 
RETURNS INT
AS
BEGIN
    DECLARE @TaskCount INT;
    SELECT @TaskCount = COUNT(*) FROM Tasks WHERE ProjectID = @ProjectID;
    RETURN @TaskCount;
END;
GO

CREATE FUNCTION dbo.fn_IsUserInProject(@UserID INT, @ProjectID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @Exists BIT;
    IF EXISTS (SELECT 1 FROM UserProjects WHERE UserID = @UserID AND ProjectID = @ProjectID)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
    RETURN @Exists;
END;
GO

CREATE FUNCTION dbo.fn_GetUserRoleInProject(@UserID INT, @ProjectID INT)
RETURNS VARCHAR(30)
AS
BEGIN
    DECLARE @RoleName VARCHAR(30);
    SELECT @RoleName = r.RoleName
    FROM UserProjects up
    JOIN Roles r ON up.RoleID = r.ID
    WHERE up.UserID = @UserID AND up.ProjectID = @ProjectID;
    RETURN @RoleName;
END;
GO

CREATE FUNCTION dbo.fn_GetOverdueTasks()
RETURNS TABLE
AS
RETURN
(
    SELECT * 
    FROM Tasks 
    WHERE DueDate < GETDATE() AND StatusID != (SELECT ID FROM Status WHERE StatusName = 'Completed')
);
GO

CREATE FUNCTION dbo.fn_GetUserTasksInProgressInProject(@UserID INT, @ProjectID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT * 
    FROM Tasks t
    WHERE 
    (
        DueDate > GETDATE() AND 
        StatusID != (SELECT ID FROM Status WHERE StatusName = 'Completed') AND
        t.AssigneeID = @UserID AND 
        t.ProjectID = @ProjectID
    )
);
GO

CREATE FUNCTION dbo.fn_GetUserTasksInProgress(@UserID INT)
RETURNS TABLE
AS
RETURN
(
    SELECT * 
    FROM Tasks t
    WHERE 
    (
        DueDate > GETDATE() AND 
        StatusID != (SELECT ID FROM Status WHERE StatusName = 'Completed') AND
        t.AssigneeID = @UserID
    )
);
GO

CREATE FUNCTION dbo.fn_IsUserExists(@UserID INT)
RETURNS BIT
AS 
BEGIN
    DECLARE @Exists BIT;
    IF EXISTS (SELECT 1 FROM Users WHERE UserID = @UserID)
        SET @Exists = 1;
    ELSE 
        SET @Exists = 0;
    RETURN @Exists;
END;
GO

CREATE FUNCTION dbo.fn_GetUserProjectsCount(@UserID INT)
RETURNS INT
AS 
BEGIN 
    DECLARE @UserProjectsCount INT;
    SELECT @UserProjectsCount = COUNT(*) FROM UserProjects WHERE MemberID = @UserID;
    RETURN @UserProjectsCount;
END;
GO

CREATE FUNCTION dbo.fn_IsProjectExists(@ProjectID INT)
RETURNS BIT 
AS 
BEGIN 
    DECLARE @Exists BIT;
    IF EXISTS (SELECT 1 FROM Projects WHERE ID = @ProjectID)
        SET @Exists = 1;
    ELSE 
        SET @Exists = 0;
    RETURN @Exists;
END;
GO

CREATE FUNCTION dbo.fn_GetProjectMemberCount(@ProjectID INT)
RETURNS INT
AS
BEGIN
    DECLARE @ProjectMemberCount INT;
    SELECT @ProjectMemberCount = COUNT(*) FROM UserProjects WHERE ProjectID = @ProjectID;
    RETURN @ProjectMemberCount;
END;
GO

CREATE FUNCTION dbo.fn_GetProjectDescription(@ProjectID INT)
RETURNS NVARCHAR
AS
BEGIN
    DECLARE @ProjectDescription NVARCHAR;
    SELECT @ProjectDescription = ProjectDescription FROM Projects WHERE ID = @ProjectID;
    RETURN @ProjectDescription;
END;
GO

CREATE FUNCTION dbo.fn_IsTaskExists(@TaskID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @Exists BIT;
    IF EXISTS (SELECT 1 FROM Tasks WHERE ID = @TaskID)
        SET @Exists = 1;
    ELSE 
        SET @Exists = 0;
    RETURN @Exists;
END;
GO

CREATE FUNCTION dbo.fn_GetProjectOwner(@ProjectID INT)
RETURNS INT
AS
BEGIN
    DECLARE @ProjectOwner INT;
    SELECT @ProjectOwner = OwnerID FROM Projects WHERE ID = @ProjectID;
    RETURN @ProjectOwner;
END;
GO

CREATE FUNCTION dbo.fn_GetTaskAssignee(@TaskID INT)
RETURNS INT
AS
BEGIN
    DECLARE @TaskAssignee INT;;
    SELECT @TaskAssignee = AssigneeID FROM Tasks WHERE ID = @TaskID;
    RETURN @TaskAssignee;
END;
GO