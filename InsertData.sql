INSERT INTO [dbo].[Users] (GitHubID) 
VALUES 
	('BradleyR-BDD'),
	('Scelo-Khoza-BBD'),
	('Andy-Nkumane-BBD'),
	('Koketso-BBD'),
	('user_gh_001'), 
	('user_gh_002'), 
	('user_gh_003'), 
	('user_gh_004'), 
	('user_gh_005');

INSERT INTO [dbo].[Projects] (ProjectName, ProjectDescription, UserID, CreatedAt) 
VALUES
	('Bradley Project', 'This is my test project', 1, GETDATE()),
	('Scelo Project', 'This is his test project', 2, GETDATE()),
	('Task Management System', 'A system to manage team tasks efficiently.', 1, '2025-02-01'),
	('DevOps Automation', 'A project to automate deployment pipelines.', 2, '2025-02-02'),
	('E-commerce Platform', 'An online store for multiple vendors.', 3, '2025-02-03');

INSERT INTO [dbo].[Roles] (RoleName) 
VALUES 
	('Admin'), 
	('Contributor'), 
	('Viewer'),
	('Guest');

INSERT INTO [dbo].[UserProjects] (ProjectID, UserID, RoleID, JoinedAt) 
VALUES 
	(1, 1, 1, '2025-02-01'),
	(1, 2, 2, '2025-02-01'),
	(2, 3, 3, '2025-02-02'),
	(3, 4, 4, '2025-02-03'),
	(3, 5, 1, '2025-02-04');

INSERT INTO [dbo].[NotificationTypes] (Notification) 
	VALUES 
	('Task Assigned'), 
	('Task Completed'), 
	('New Project Added'), 
	('New Comment');

INSERT INTO [dbo].[Status] (StatusName) 
VALUES 
	('Unconfirmed'),
	('To Do'),
	('In Progress'),
	('Completed');

INSERT INTO [dbo].[Tasks] 
	(AssigneeID, TaskName, TaskDescription, DueDate, Priority, ProjectID, StatusID, ConfirmationDate, CompleitionDate, CreatedAt) 
VALUES 
	(1, 'Design Database Schema', 'Create ERD and define tables.', '2025-02-15', 2, 1, 1, '2025-02-05', NULL, '2025-02-02'),
	(2, 'Develop API Endpoints', 'Implement CRUD for users.', '2025-02-20', 3, 1, 2, '2025-02-07', NULL, '2025-02-03'),
	(3, 'Setup CI/CD Pipeline', 'Configure automated builds and deployments.', '2025-02-22', 1, 2, 2, '2025-02-08', NULL, '2025-02-05'),
	(4, 'Write Unit Tests', 'Ensure test coverage for backend.', '2025-02-25', 4, 3, 3, '2025-02-09', '2025-02-18', '2025-02-06'),
	(5, 'Frontend UI Design', 'Create wireframes and initial UI components.', '2025-02-28', 2, 3, 1, '2025-02-10', NULL, '2025-02-07');

INSERT INTO [dbo].[Labels] (LabelName, ProjectID) 
VALUES 
	('Bug', 1), 
	('Feature', 1), 
	('Improvement', 2), 
	('Documentation', 3);

INSERT INTO [dbo].[TaskLabels] (TaskID, LabelID) 
VALUES 
	(1, 1), 
	(2, 2), 
	(3, 3), 
	(4, 4), 
	(5, 1);

INSERT INTO [dbo].[Notifications] (UserID, ProjectID, TaskID, NotificationTypeID, Message, IsRead, CreatedAt) 
VALUES 
	(1, 1, 1, 1, 'You have been assigned a new task: Design Database Schema.', 0, '2025-02-05'),
	(2, 1, 2, 2, 'The task "Develop API Endpoints" has been marked as completed.', 0, '2025-02-06'),
	(3, 2, 3, 3, 'A new project "DevOps Automation" has been added.', 1, '2025-02-07'),
	(4, 3, 4, 4, 'A new comment has been added to your task "Write Unit Tests".', 0, '2025-02-08');

INSERT INTO [dbo].[StatusTrack] (StatusID, TaskID, StartedAt) 
VALUES 
	(1, 1, '2025-02-05'),
	(2, 2, '2025-02-07'),
	(3, 3, '2025-02-08'),
	(4, 4, '2025-02-09'),
	(2, 5, '2025-02-10');