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