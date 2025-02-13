
--- For CREATING A PROJECT AND ASSIGNING A OWNER

EXEC sproc_CreateProject
    @ProjectName = '',
    @ProjectDescription = '',
    @UserID = 123;
---------------------------------------




--- FOR ADDING USER TO PROJECT----

EXEC sp_AddUserToProject
    @UserID = 3,
    @ProjectID = 2,
    @RoleID = 2;
----------------------------------



----------For creating a task-----------
EXEC sp_CreateTask
    @TaskName = '',
    @TaskDescription = '',
    @DueDate = '',
    @Priority = '',
    @ProjectID = 1,
    @AssigneeID = 1,
    @StatusID = 1;
---------------------------------------




--------------for updating a task-------------
EXEC sp_UpdateTaskStatus
    @TaskID = 1,
    @NewStatusID = 2;
---------------------------------------------




------------------fOR marking notification as read---------------
EXEC sp_MarkNotificationAsRead
    @NotificationID = 2
-----------------------------------------------------------------