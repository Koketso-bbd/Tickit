﻿INSERT INTO [tickit].[Priority]
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
    ('DummyUser1'),
    ('CodeMaster23'),
    ('TechieGuy99'),
    ('DevWizardX'),
    ('PseudoCoder'),
    ('TestUserABC'),
    ('JaneDoeDev'),
    ('JohnSmith42'),
    ('RandomCoder777'),
    ('ScriptJunkieX'),
    ('BitFlipper99'),
    ('StackOverflowed'),
    ('NullPointerX'),
    ('CloudNinja24'),
    ('AI_Explorer'),
    ('CyberSamurai88');
GO

INSERT INTO [tickit].[Projects]
	(ProjectName, ProjectDescription, OwnerID)

VALUES
	('TaskIT Planning', 'We will be planning the Database of our project guys 😊🤙', 1),
	('GruveIT API Work', 'Let us work on the API', 5),
	('Woggle Deployment', 'Steps to take to deploy wriggle', 8),
	('Dustify Front End', 'I think we need to make it pretty ❤️', 11),
	('My Personal Blah List', NULL, 6),
	('Blah Check List', 'What is it that I need to do?', 1);
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
	(2, 'Research Stored Procedures', 'Deep dive into stored procedures for us', GETDATE(), 3, 1, 2),
	(3, 'Research UDFs', 'Please read more about UDFs', GETDATE(), 3, 1, 2),
	(3, 'Implement UDFs', 'Deep dive into stored procedures for us', GETDATE(), 3, 1, 2),
	(4, 'Research and Implement Views', 'Please implement views for our database', GETDATE(), 3, 1, 2),
	(5, 'Implement API Endpoints', NULL, GETDATE(), 3, 2, 1),
	(14, 'Write tests for the API', 'Make sure there is high test coverage for our APIs', GETDATE(), 4, 2, 1),
	(12, 'Implement Dark Mode Toggle', 'Allow users to switch themes', GETDATE(), 3, 4, 1),
    (9, 'Enhance Mobile Responsiveness', 'Make UI fully adaptive to mobile devices', GETDATE(), 3, 4, 2),
	(1, 'Fix Plumbing Issue', 'Check and repair leaking taps in the kitchen', GETDATE(), 3, 6, 2),
    (1, 'Buy Groceries', 'Get weekly groceries for the house', GETDATE(), 1, 6, 2),
    (1, 'Schedule Cleaning Service', 'Book a cleaning company for deep cleaning', GETDATE(), 2, 6, 1);


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