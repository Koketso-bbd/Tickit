MERGE INTO [dbo].[Priority] AS target
USING (VALUES 
    ('Low'), 
    ('Medium'), 
    ('High'), 
    ('Urgent')
) AS source (PriorityLevel)
ON target.PriorityLevel = source.PriorityLevel
WHEN NOT MATCHED THEN 
    INSERT (PriorityLevel) VALUES (source.PriorityLevel);

MERGE INTO [dbo].[Roles] AS target
USING (VALUES 
    ('Admin'), 
    ('Contributor'), 
    ('Viewer')
) AS source (RoleName)
ON target.RoleName = source.RoleName
WHEN NOT MATCHED THEN 
    INSERT (RoleName) VALUES (source.RoleName);

MERGE INTO [dbo].[Status] AS target
USING (VALUES 
    ('Unconfirmed'), 
    ('To Do'), 
    ('In Progress'), 
    ('Completed')
) AS source (StatusName)
ON target.StatusName = source.StatusName
WHEN NOT MATCHED THEN 
    INSERT (StatusName) VALUES (source.StatusName);

MERGE INTO [dbo].[NotificationTypes] AS target
USING (VALUES 
    ('Task Assigned'), 
    ('Task Completed'), 
    ('Added to Project')
) AS source (NotificationName)
ON target.NotificationName = source.NotificationName
WHEN NOT MATCHED THEN 
    INSERT (NotificationName) VALUES (source.NotificationName);

MERGE INTO [dbo].[Users] AS target
USING (VALUES 
    ('DummyUser1'), ('CodeMaster23'), ('TechieGuy99'), ('DevWizardX'), 
    ('PseudoCoder'), ('TestUserABC'), ('JaneDoeDev'), ('JohnSmith42'), 
    ('RandomCoder777'), ('ScriptJunkieX'), ('BitFlipper99'), ('StackOverflowed'), 
    ('NullPointerX'), ('CloudNinja24'), ('AI_Explorer'), ('CyberSamurai88')
) AS source (GitHubID)
ON target.GitHubID = source.GitHubID
WHEN NOT MATCHED THEN 
    INSERT (GitHubID) VALUES (source.GitHubID);

MERGE INTO [dbo].[Projects] AS target
USING (VALUES 
    ('TaskIT Planning', 'We will be planning the Database of our project guys 😊🤙', 1),
    ('GruveIT API Work', 'Let us work on the API', 5),
    ('Woggle Deployment', 'Steps to take to deploy wriggle', 8),
    ('Dustify Front End', 'I think we need to make it pretty ❤️', 11),
    ('My Personal Blah List', NULL, 6),
    ('Blah Check List', 'What is it that I need to do?', 1)
) AS source (ProjectName, ProjectDescription, OwnerID)
ON target.ProjectName = source.ProjectName
WHEN NOT MATCHED THEN 
    INSERT (ProjectName, ProjectDescription, OwnerID) 
    VALUES (source.ProjectName, source.ProjectDescription, source.OwnerID);

MERGE INTO [dbo].[UserProjects] AS target
USING (VALUES 
    (1, 2, 1), (1, 3, 1), (1, 4, 1), (1, 6, 3), 
    (2, 5, 1), (2, 7, 2), (2, 14, 2), (2, 12, 2), 
    (3, 8, 1), (3, 9, 3), (3, 6, 2), (3, 10, 2)
) AS source (ProjectID, MemberID, RoleID)
ON target.ProjectID = source.ProjectID AND target.MemberID = source.MemberID
WHEN NOT MATCHED THEN 
    INSERT (ProjectID, MemberID, RoleID) 
    VALUES (source.ProjectID, source.MemberID, source.RoleID);

MERGE INTO [dbo].[Tasks] AS target
USING (VALUES 
    (2, 'Research Stored Procedures', 'Deep dive into stored procedures for us', GETDATE(), 3, 1, 2),
    (3, 'Research UDFs', 'Please read more about UDFs', GETDATE(), 3, 1, 2),
    (3, 'Implement UDFs', 'Deep dive into stored procedures for us', GETDATE(), 3, 1, 2),
    (4, 'Research and Implement Views', 'Please implement views for our database', GETDATE(), 3, 1, 2),
    (5, 'Implement API Endpoints', NULL, DATEADD(day, 1, GETDATE()), 3, 2, 1),
    (14, 'Write tests for the API', 'Make sure there is high test coverage for our APIs', GETDATE(), 4, 2, 1),
    (12, 'Implement Dark Mode Toggle', 'Allow users to switch themes', GETDATE(), 3, 4, 1),
    (9, 'Enhance Mobile Responsiveness', 'Make UI fully adaptive to mobile devices', GETDATE(), 3, 4, 2),
    (1, 'Fix Plumbing Issue', 'Check and repair leaking taps in the kitchen', GETDATE(), 3, 6, 2),
    (1, 'Buy Groceries', 'Get weekly groceries for the house', GETDATE(), 1, 6, 2),
    (1, 'Schedule Cleaning Service', 'Book a cleaning company for deep cleaning', GETDATE(), 2, 6, 1)
) AS source (AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)
ON target.TaskName = source.TaskName AND target.ProjectID = source.ProjectID
WHEN NOT MATCHED THEN 
    INSERT (AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)
    VALUES (source.AssigneeID, source.TaskName, source.TaskDescription, source.DueDate, source.PriorityID, source.ProjectID, source.StatusID);

MERGE INTO [dbo].[Labels] AS target
USING (VALUES ('Bug'), ('Feature'), ('Urgent'), ('UI'), ('Backend'), ('Enhancement')) AS source (LabelName)
ON target.LabelName = source.LabelName
WHEN NOT MATCHED THEN 
    INSERT (LabelName) VALUES (source.LabelName);

MERGE INTO [dbo].[ProjectLabels] AS target
USING (VALUES (1, 2), (1, 3), (2, 1)) AS source (ProjectID, LabelID)
ON target.ProjectID = source.ProjectID AND target.LabelID = source.LabelID
WHEN NOT MATCHED THEN 
    INSERT (ProjectID, LabelID) VALUES (source.ProjectID, source.LabelID);

MERGE INTO [dbo].[TaskLabels] AS target
USING (VALUES (1, 1), (1, 2), (2, 3)) AS source (TaskID, ProjectLabelID)
ON target.TaskID = source.TaskID AND target.ProjectLabelID = source.ProjectLabelID
WHEN NOT MATCHED THEN 
    INSERT (TaskID, ProjectLabelID) VALUES (source.TaskID, source.ProjectLabelID);