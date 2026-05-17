using JobService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.Infrastructure.Data;

public class JobServiceDbContext : DbContext
{
    public JobServiceDbContext(DbContextOptions<JobServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobRequisition> JobRequisitions => Set<JobRequisition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobRequisition>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.RoleTitle)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.SourceUrl)
                .HasMaxLength(2048);

            entity.Property(e => e.CompanyCareerPortalUrl)
                .HasMaxLength(2048);

            entity.Property(e => e.JobDescription)
                .HasColumnType("text");

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.InterviewDate)
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Index on UserId for fast per-user queries
            entity.HasIndex(e => e.UserId);

            // Index on UserId + IsDeleted for dashboard queries
            entity.HasIndex(e => new { e.UserId, e.IsDeleted });

            // Soft delete filter - never return deleted records
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.ToTable("job_requisitions");
        });
    }
}
