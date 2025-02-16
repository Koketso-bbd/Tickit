USE TickItDB;
GO

CREATE VIEW [dbo].[vw_ProjectWithOwners]

AS
SELECT 
	p.ID AS ProjectID,
	p.ProjectName,
	p.ProjectDescription,
	u.GitHubID AS OwnerID

FROM Projects p
JOIN Users u ON p.OwnerID = u.ID;
GO


CREATE VIEW [dbo].[vw_UserProjectRoles]

AS
SELECT
	up.ID AS UserProjectID,
	r.RoleName,
	r.ID AS RoleID,
	p.ProjectName,
	u.GitHubID

FROM UserProjects up
JOIN Roles r ON up.RoleID  = r.ID
JOIN Projects p ON up.ProjectID = p.ID
JOIN Users u ON up.MemberID = u.ID;
GO

CREATE VIEW [dbo].[vw_TasksWithAssignees]
AS 
SELECT
    t.ID AS TaskID,
    t.TaskName,
    t.TaskDescription,
    u.GitHubID AS AssigneeGitHubID,
    p.ProjectName,
    s.StatusName,
    t.DueDate,
    t.PriorityID
FROM Tasks t
JOIN Users u ON t.AssigneeID = u.ID
JOIN Projects p ON t.ProjectID = p.ID
JOIN Status s ON t.StatusID = s.ID;
GO

CREATE VIEW [dbo].[vw_OverdueTasks]
AS
SELECT
    t.ID AS TaskID,
    t.TaskName,
    u.GitHubID AS AssigneeGitHubID,
    p.ProjectName,
    s.StatusName,
    t.DueDate
FROM Tasks t
JOIN Users u ON t.AssigneeID = u.ID
JOIN Projects p ON t.ProjectID = p.ID
JOIN Status s ON t.StatusID = s.ID
WHERE t.DueDate < GETDATE() AND s.StatusName != 'Completed';
GO

CREATE VIEW [dbo].[vw_UnreadNotifications] 
AS
SELECT 
    n.ID AS NotificationID,
    u.GitHubID AS UserGitHubID,
    p.ProjectName,
    t.TaskName,
    nt.NotificationName AS NotificationType,
    n.Message,
    n.CreatedAt
FROM Notifications n
JOIN Users u ON n.UserID = u.ID
JOIN Projects p ON n.ProjectID = p.ID
LEFT JOIN Tasks t ON n.TaskID = t.ID
JOIN NotificationTypes nt ON n.NotificationTypeID = nt.ID
WHERE n.IsRead = 0;
GO

CREATE VIEW [dbo].[vw_TaskStatusHistory] 
AS
SELECT 
    st.TaskID,
    t.TaskName,
    s.StatusName,
    st.UpdatedAt
FROM StatusTrack st
JOIN Tasks t ON st.TaskID = t.ID
JOIN Status s ON st.StatusID = s.ID;
GO