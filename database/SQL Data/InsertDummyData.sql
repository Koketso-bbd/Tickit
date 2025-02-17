USE TickItDB;
GO

INSERT INTO [tickit].[Priority]
	(PriorityLevel)

VALUES
	('Low'),
	('Medium'),
	('High'),
	('Urgent');
GO


INSERT INTO [tickit].[Roles]
	(RoleName)

VALUES
	('Admin'),
	('Contributor'),
	('Viewer');
GO

INSERT INTO [tickit].[Status]
	(StatusName)

VALUES
	('Unconfirmed'),
	('To Do'),
	('In Progress'),
	('Completed');
GO

INSERT INTO [tickit].[NotificationTypes]
	(NotificationName)

VALUES
	('Task Assigned'),
	('Task Completed'),
	('Added to Project');
GO

INSERT INTO [tickit].[Users]
	(GitHubID)
VALUES
	('BradleyR-BBD'),
	('Scelo-Khoza-BBD'),
	('Andy-Nkumane-BDD'),
	('Koketso-BBD'),
	('TadimaM-BBD'),
	('BBDBhembe'),
	('LiamH-BBD'),
	('AndreB-BBD'),
	('MaseloS-BBD'),
	('MduD-BBD'),
	('MiaD-BBD'),
	('SpheM-BBD'),
	('NadrianP-BBD'),
	('TshilidziM-BBD'),
	('KamoT-BBD'),
	('AzaelL-BBD');
GO

INSERT INTO [tickit].[Projects]
	(ProjectName, ProjectDescription, OwnerID)

VALUES
	('TaskIT Planning', 'We will be planning the Database of our project guys 😊🤙', 1),
	('GrabIT API Work', 'Let us work on the API', 5),
	('Wriggle Deployment', 'Steps to take to deploy wriggle', 8),
	('Listify Front End', 'I think we need to make it pretty ❤️', 11),
	('My Personal Task List', NULL, 6),
	('House Check List', 'What is it that I need to do?', 1);
GO

INSERT INTO [tickit].[UserProjects]
	(ProjectID, MemberID, RoleID)

VALUES
	(1, 2, 1),
	(1, 3, 1),
	(1, 4, 1),
	(1, 6, 3),
	(2, 5, 1),
	(2, 7, 2),
	(2, 14, 2),
	(2, 12, 2),
	(3, 8, 1),
	(3, 9, 3),
	(3, 6, 2),
	(3, 10, 2);
GO

INSERT INTO [tickit].[Tasks]
	(AssigneeID, TaskName, TaskDescription, DueDate, PriorityID, ProjectID, StatusID)

VALUES
	(2, 'Research Stored Procedures', 'Deep dive into stored procedures for us', '2025-02-14', 3, 1, 2),
	(3, 'Research UDFs', 'Please read more about UDFs', '2025-02-14', 3, 1, 2),
	(3, 'Implement UDFs', 'Deep dive into stored procedures for us', '2025-02-14', 3, 1, 2),
	(4, 'Research and Implement Views', 'Please implement views for our database', '2025-02-14', 3, 1, 2),
	(5, 'Implement API Endpoints', NULL, '2025-02-23', 3, 2, 1),
	(14, 'Write tests for the API', 'Make sure there is high test coverage for our APIs', '2025-02-25', 4, 2, 1),
	(12, 'Implement Dark Mode Toggle', 'Allow users to switch themes', '2025-02-26', 3, 4, 1),
    (9, 'Enhance Mobile Responsiveness', 'Make UI fully adaptive to mobile devices', '2025-02-27', 3, 4, 2),
	(1, 'Fix Plumbing Issue', 'Check and repair leaking taps in the kitchen', '2025-02-15', 3, 6, 2),
    (1, 'Buy Groceries', 'Get weekly groceries for the house', '2025-02-16', 1, 6, 2),
    (1, 'Schedule Cleaning Service', 'Book a cleaning company for deep cleaning', '2025-02-17', 2, 6, 1);


INSERT INTO Labels 
	(LabelName)

VALUES ('Bug'), ('Feature'), ('Urgent'), ('UI'), ('Backend'), ('Enhancement');


INSERT INTO ProjectLabels 
	(ProjectID, LabelID)

VALUES 
	(1, 2),
	(1, 3),
	(2, 1);

INSERT INTO [tickit].[TaskLabels] 
	(TaskID, ProjectLabelID)
VALUES (1, 1),
       (1, 2),
       (2, 3);