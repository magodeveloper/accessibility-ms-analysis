using Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analysis.Infrastructure
{
    public class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options) { }

        public DbSet<Analysis.Domain.Entities.Analysis> Analyses { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Error> Errors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Analysis.Domain.Entities.Analysis>(entity =>
            {
                entity.ToTable("ANALYSIS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DateAnalysis).HasColumnName("date_analysis");
                entity.Property(e => e.ContentType)
                    .HasColumnName("content_type")
                    .HasConversion(new EnumToStringConverter<ContentType>())
                    .HasMaxLength(8)
                    .IsRequired();
                entity.Property(e => e.ContentInput).HasColumnName("content_input");
                entity.Property(e => e.SourceUrl).HasColumnName("source_url");
                entity.Property(e => e.ToolUsed)
                    .HasColumnName("tool_used")
                    .HasConversion(new EnumToStringConverter<ToolUsed>())
                    .HasMaxLength(12)
                    .IsRequired();
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion(new EnumToStringConverter<AnalysisStatus>())
                    .HasMaxLength(8)
                    .IsRequired();
                entity.Property(e => e.SummaryResult).HasColumnName("summary_result");
                entity.Property(e => e.ResultJson).HasColumnName("result_json");
                entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
                entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
                entity.Property(e => e.WcagVersion)
                    .HasColumnName("wcag_version")
                    .HasConversion(new EnumToStringConverter<WcagVersion>())
                    .HasMaxLength(4)
                    .IsRequired();
                entity.Property(e => e.WcagLevel)
                    .HasColumnName("wcag_level")
                    .HasConversion(new EnumToStringConverter<WcagLevel>())
                    .HasMaxLength(3)
                    .IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasMany(a => a.Results).WithOne(r => r.Analysis).HasForeignKey(r => r.AnalysisId);
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("RESULTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnalysisId).HasColumnName("analysis_id");
                entity.Property(e => e.WcagCriterionId).HasColumnName("wcag_criterion_id");
                entity.Property(e => e.WcagCriterion).HasColumnName("wcag_criterion");
                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasConversion(new EnumToStringConverter<ResultLevel>())
                    .HasMaxLength(15)
                    .IsRequired();
                entity.Property(e => e.Severity)
                    .HasColumnName("severity")
                    .HasConversion(new EnumToStringConverter<Severity>())
                    .HasMaxLength(8)
                    .IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasMany(r => r.Errors).WithOne(e => e.Result).HasForeignKey(e => e.ResultId);
            });

            modelBuilder.Entity<Error>(entity =>
            {
                entity.ToTable("ERRORS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ResultId).HasColumnName("result_id");
                entity.Property(e => e.WcagCriterionId).HasColumnName("wcag_criterion_id");
                entity.Property(e => e.ErrorCode).HasColumnName("error_code");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Location).HasColumnName("location");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });
        }
    }
}