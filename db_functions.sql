CREATE FUNCTION dbo.fn_GetTaskCount(@ProjectID INT) 
RETURNS INT
AS
BEGIN
    DECLARE @TaskCount INT;
    SELECT @TaskCount = COUNT(*) FROM Tasks WHERE ProjectID = @ProjectID;
    RETURN @TaskCount;
END;
GO

-- SELECT dbo.fn_GetTaskCount(2);
-- GO

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

-- SELECT dbo.fn_IsUserInProject(2,1);
-- GO

CREATE FUNCTION fn_GetUserRoleInProject(@UserID INT, @ProjectID INT)
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

-- SELECT dbo.fn_GetUserRoleInProject(5,3);
-- GO