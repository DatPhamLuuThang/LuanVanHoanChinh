using CoreEntities.SchoolMgntModel;
using Microsoft.EntityFrameworkCore;


namespace SchoolManagement.Data.SchoolManagement;

public class SchoolManagementDbContext : DbContext
{
    
    public SchoolManagementDbContext(DbContextOptions<SchoolManagementDbContext> options) : base(options)
    {
    }
    
    public virtual DbSet<SchoolDetail>? SchoolDetails { get; set; }
    public virtual DbSet<Teacher>? Teacher { get; set; }
    public virtual DbSet<ClassRoom>? ClassRoom { get; set; }
    public virtual DbSet<Student>? Student { get; set; }
    public virtual DbSet<Parents>? Parents { get; set; }
    public virtual DbSet<Schedule>? Schedule { get; set; }
    public virtual DbSet<Semester>? Semester { get; set; }
    public virtual DbSet<Subject>? Subject { get; set; }
    public virtual DbSet<Menu>? Menu { get; set; }
    public virtual DbSet<Transcript>? Transcript { get; set; }
    public virtual DbSet<Violation>? Violation { get; set; }
    public virtual DbSet<News>? News { get; set; }
    public virtual DbSet<ScheduleTime>? ScheduleTime { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SchoolDetail>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<Teacher>().Property(e => e.Id).IsRequired();

        modelBuilder.Entity<ClassRoom>().Property(e => e.Id).IsRequired();

        modelBuilder.Entity<Student>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<Parents>().Property(e => e.Id).IsRequired();
       
        modelBuilder.Entity<Schedule>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<Subject>().Property(e => e.Id).IsRequired();

        modelBuilder.Entity<Semester>().Property(e => e.Id).IsRequired();
      
        modelBuilder.Entity<Transcript>().Property(e => e.Id).IsRequired();
        modelBuilder.Entity<Transcript>().Property(e => e.Value).HasColumnType("decimal(18,2)");
        
        modelBuilder.Entity<Violation>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<News>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<ScheduleTime>().Property(e => e.Id).IsRequired();
        
        modelBuilder.Entity<Menu>().Property(e => e.Id).IsRequired();
        
        
        modelBuilder.Entity<Menu>().HasOne(d => d.Parent)
            .WithMany(p => p.Child)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        modelBuilder.Entity<ClassRoom>().HasOne(d => d.SchoolDetail)
            .WithMany(p => p.ClassRooms)
            .HasForeignKey(d => d.SchoolDetailId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ClassRoom_SchoolDetail");
        modelBuilder.Entity<ClassRoom>().HasOne(d => d.Teacher)
            .WithMany(p => p.ClassRooms)
            .HasForeignKey(d => d.TeacherId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ClassRoom_Teacher");


        modelBuilder.Entity<Student>().HasOne(d => d.ClassRoom)
            .WithMany(p => p.Students)
            .HasForeignKey(d => d.ClassRoomId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Student_ClassRoom");
       
        modelBuilder.Entity<Student>().HasOne(d => d.Parents)
            .WithMany(p => p.Students)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Student_Parents");
        

        modelBuilder.Entity<Transcript>().HasOne(d => d.Student)
            .WithMany(p => p.Transcripts)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Transcript_Student");
        modelBuilder.Entity<Transcript>().HasOne(d => d.Subject)
            .WithMany(p => p.Transcripts)
            .HasForeignKey(d => d.SubjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Transcript_Subject");
        modelBuilder.Entity<Transcript>().HasOne(d => d.Semester)
            .WithMany(p => p.Transcripts)
            .HasForeignKey(d => d.SemesterId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Transcript_Semester");
        
        
        modelBuilder.Entity<Subject>().HasOne(d => d.Teacher)
            .WithMany(p => p.Subjects)
            .HasForeignKey(d => d.TeacherId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Subject_Teacher");
        

        modelBuilder.Entity<Schedule>().HasOne(d => d.ClassRoom)
            .WithMany(p => p.Schedules)
            .HasForeignKey(d => d.ClassRoomId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Schedule_ClassRoom");
        modelBuilder.Entity<Schedule>().HasOne(d => d.Subject)
            .WithMany(p => p.Schedules)
            .HasForeignKey(d => d.SubjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Schedule_Subject");
        modelBuilder.Entity<Schedule>().HasOne(d => d.Semester)
            .WithMany(p => p.Schedules)
            .HasForeignKey(d => d.SemesterId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Schedule_Semester");
        modelBuilder.Entity<Schedule>().HasOne(d => d.ScheduleTime)
            .WithMany(p => p.Schedules)
            .HasForeignKey(d => d.ScheduleTimeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Schedule_ScheduleTime");
        
        
        modelBuilder.Entity<Violation>().HasOne(d => d.Student)
            .WithMany(p => p.Violations)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Violation_Student");
        

    }
}