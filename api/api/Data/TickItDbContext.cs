using System;
using System.Collections.Generic;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public partial class TickItDbContext : DbContext
{
    public TickItDbContext()
    {
    }

    public TickItDbContext(DbContextOptions<TickItDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Label> Labels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectLabel> ProjectLabels { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<StatusTrack> StatusTracks { get; set; }

    public virtual DbSet<Models.Task> Tasks { get; set; }

    public virtual DbSet<TaskLabel> TaskLabels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProject> UserProjects { get; set; }

    public virtual DbSet<VwOverdueTask> VwOverdueTasks { get; set; }

    public virtual DbSet<VwProjectWithOwner> VwProjectWithOwners { get; set; }

    public virtual DbSet<VwTaskStatusHistory> VwTaskStatusHistories { get; set; }

    public virtual DbSet<VwTasksWithAssignee> VwTasksWithAssignees { get; set; }

    public virtual DbSet<VwUnreadNotification> VwUnreadNotifications { get; set; }

    public virtual DbSet<VwUserProjectRole> VwUserProjectRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tickit");

        modelBuilder.Entity<Label>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LabelName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.NotificationTypeId).HasColumnName("NotificationTypeID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.NotificationType).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_NotificationTypes");

            entity.HasOne(d => d.Project).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Projects");

            entity.HasOne(d => d.Task).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK_Notifications_Tasks");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasIndex(e => e.NotificationName, "UQ_NotificationName").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NotificationName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Priority>(entity =>
        {
            entity.ToTable("Priority");

            entity.HasIndex(e => e.PriorityLevel, "UQ_PriorityLevel").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PriorityLevel)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");
            entity.Property(e => e.ProjectDescription).HasMaxLength(1500);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.Projects)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Users");
        });

        modelBuilder.Entity<ProjectLabel>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.LabelId }, "UQ_ProjectLabel").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LabelId).HasColumnName("LabelID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");

            entity.HasOne(d => d.Label).WithMany(p => p.ProjectLabels)
                .HasForeignKey(d => d.LabelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectLabels_Label");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectLabels)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProjectLabels_Project");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.RoleName, "UQ_RoleName").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");

            entity.HasIndex(e => e.StatusName, "UQ_StatusName").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StatusTrack>(entity =>
        {
            entity.ToTable("StatusTrack");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Status).WithMany(p => p.StatusTracks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusTrack_Status");

            entity.HasOne(d => d.Task).WithMany(p => p.StatusTracks)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatusTrack_Task");
        });

        modelBuilder.Entity<Models.Task>(entity =>
        {
            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_DueDateCantBeSetInThePast");
                    tb.HasTrigger("trg_InsertStatusTrack");
                    tb.HasTrigger("trg_NotifyUserOnTaskAssignment");
                    tb.HasTrigger("trg_PreventDuplicateUserOnTask");
                    tb.HasTrigger("trg_PreventStatusDowngrade");
                });

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AssigneeId).HasColumnName("AssigneeID");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.PriorityId).HasColumnName("PriorityID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TaskDescription).HasMaxLength(1000);
            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Assignee).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Users");

            entity.HasOne(d => d.Priority).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Priority");

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Project");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Status");
        });

        modelBuilder.Entity<TaskLabel>(entity =>
        {
            entity.HasIndex(e => new { e.TaskId, e.ProjectLabelId }, "UQ_TaskLabels").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProjectLabelId).HasColumnName("ProjectLabelID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.HasOne(d => d.ProjectLabel).WithMany(p => p.TaskLabels)
                .HasForeignKey(d => d.ProjectLabelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskLabels_Labels");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskLabels)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaskLabels_Tasks");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.GitHubId, "UQ_GitHubID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GitHubId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("GitHubID");
        });

        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_NotifyUserOnProjectAdded");
                    tb.HasTrigger("trg_PreventDuplicatUserInProject");
                });

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Member).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProjects_Users");

            entity.HasOne(d => d.Project).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProjects_Project");

            entity.HasOne(d => d.Role).WithMany(p => p.UserProjects)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProjects_Roles");
        });

        modelBuilder.Entity<VwOverdueTask>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_OverdueTasks");

            entity.Property(e => e.AssigneeGitHubId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("AssigneeGitHubID");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwProjectWithOwner>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ProjectWithOwners");

            entity.Property(e => e.OwnerId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("OwnerID");
            entity.Property(e => e.ProjectDescription).HasMaxLength(1500);
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwTaskStatusHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TaskStatusHistory");

            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<VwTasksWithAssignee>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TasksWithAssignees");

            entity.Property(e => e.AssigneeGitHubId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("AssigneeGitHubID");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.PriorityId).HasColumnName("PriorityID");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StatusName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TaskDescription).HasMaxLength(1000);
            entity.Property(e => e.TaskId).HasColumnName("TaskID");
            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwUnreadNotification>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UnreadNotifications");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Message).HasMaxLength(255);
            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TaskName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserGitHubId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("UserGitHubID");
        });

        modelBuilder.Entity<VwUserProjectRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserProjectRoles");

            entity.Property(e => e.GitHubId)
                .HasMaxLength(39)
                .IsUnicode(false)
                .HasColumnName("GitHubID");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserProjectId).HasColumnName("UserProjectID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
