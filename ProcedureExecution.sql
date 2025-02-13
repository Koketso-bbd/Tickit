
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
