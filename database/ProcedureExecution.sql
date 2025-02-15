-- For CREATING A PROJECT AND ASSIGNING A OWNER
EXEC sproc_CreateProject
    @ProjectName = '',
    @ProjectDescription = '',
    @UserID = 123;

-- FOR ADDING USER TO PROJECT
EXEC sp_AddUserToProject
    @UserID = 3,
    @ProjectID = 2,
    @RoleID = 2;

--For creating a task
EXEC sp_CreateTask
    @TaskName = '',
    @TaskDescription = '',
    @DueDate = '',
    @Priority = '',
    @ProjectID = 1,
    @AssigneeID = 1,
    @StatusID = 1;

-- for updating a task
EXEC sp_UpdateTaskStatus
    @TaskID = 1,
    @NewStatusID = 2;

-- for marking notification as read
EXEC sp_MarkNotificationAsRead
    @NotificationID = 2

-- for removing user from a project
EXEC sp_RemoveUserFromProject
    @UserID = 1,
    @ProjectID = 3;

-- FOR getting all users tasks
EXEC sp_GetUserTasks
    @UserID = 4;

-- FOR GETTING UNREAD NOTIFICATIONS
EXEC sp_GetUserNotifications
    @UserID = 3;

-- for creating a label
EXEC sp_CreateLabel
    @LabelName = '',
    @ProjectID = 3;

-- FOR ASSIGNING LABEL TO TASK
EXEC sp_AddLabelToTask
    @TaskID = 2,
    @LabelID = 1;