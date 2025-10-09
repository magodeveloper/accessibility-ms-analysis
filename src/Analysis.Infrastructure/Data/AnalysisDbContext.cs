using Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analysis.Infrastructure.Data;

public class AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : DbContext(options)
{
    public DbSet<Analysis.Domain.Entities.Analysis> Analyses { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<Error> Errors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ANALYSIS
        _ = modelBuilder.Entity<Analysis.Domain.Entities.Analysis>(entity =>
        {
            _ = entity.ToTable("analysis");
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.UserId).HasColumnName("user_id");
            _ = entity.Property(e => e.DateAnalysis).HasColumnName("date_analysis");
            _ = entity.Property(e => e.ContentType)
                .HasColumnName("content_type")
                .HasConversion(new EnumToStringConverter<ContentType>())
                .HasMaxLength(8)
                .IsRequired();
            _ = entity.Property(e => e.ContentInput).HasColumnName("content_input");
            _ = entity.Property(e => e.SourceUrl).HasColumnName("source_url");
            _ = entity.Property(e => e.ToolUsed)
                .HasColumnName("tool_used")
                .HasConversion(new EnumToStringConverter<ToolUsed>())
                .HasMaxLength(12)
                .IsRequired();
            _ = entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(new EnumToStringConverter<AnalysisStatus>())
                .HasMaxLength(8)
                .IsRequired();
            _ = entity.Property(e => e.SummaryResult).HasColumnName("summary_result");
            _ = entity.Property(e => e.ResultJson).HasColumnName("result_json");
            _ = entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            _ = entity.Property(e => e.WcagVersion)
                .HasColumnName("wcag_version")
                .HasMaxLength(3)
                .IsRequired();
            _ = entity.Property(e => e.WcagLevel)
                .HasColumnName("wcag_level")
                .HasConversion(new EnumToStringConverter<WcagLevel>())
                .HasMaxLength(3)
                .IsRequired();
            _ = entity.Property(e => e.AxeViolations).HasColumnName("axe_violations").HasDefaultValue(0);
            _ = entity.Property(e => e.AxeNeedsReview).HasColumnName("axe_needs_review").HasDefaultValue(0);
            _ = entity.Property(e => e.AxeRecommendations).HasColumnName("axe_recommendations").HasDefaultValue(0);
            _ = entity.Property(e => e.AxePasses).HasColumnName("axe_passes").HasDefaultValue(0);
            _ = entity.Property(e => e.AxeIncomplete).HasColumnName("axe_incomplete").HasDefaultValue(0);
            _ = entity.Property(e => e.AxeInapplicable).HasColumnName("axe_inapplicable").HasDefaultValue(0);
            _ = entity.Property(e => e.EaViolations).HasColumnName("ea_violations").HasDefaultValue(0);
            _ = entity.Property(e => e.EaNeedsReview).HasColumnName("ea_needs_review").HasDefaultValue(0);
            _ = entity.Property(e => e.EaRecommendations).HasColumnName("ea_recommendations").HasDefaultValue(0);
            _ = entity.Property(e => e.EaPasses).HasColumnName("ea_passes").HasDefaultValue(0);
            _ = entity.Property(e => e.EaIncomplete).HasColumnName("ea_incomplete").HasDefaultValue(0);
            _ = entity.Property(e => e.EaInapplicable).HasColumnName("ea_inapplicable").HasDefaultValue(0);
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.HasMany(a => a.Results).WithOne(r => r.Analysis).HasForeignKey(r => r.AnalysisId);
            // Índices sugeridos
            _ = entity.HasIndex(e => e.UserId).HasDatabaseName("idx_analysis_user");
            _ = entity.HasIndex(e => e.Status).HasDatabaseName("idx_analysis_status");
            _ = entity.HasIndex(e => e.DateAnalysis).HasDatabaseName("idx_analysis_date");
        });

        // RESULTS
        _ = modelBuilder.Entity<Result>(entity =>
        {
            _ = entity.ToTable("results");
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.AnalysisId).HasColumnName("analysis_id");
            _ = entity.Property(e => e.WcagCriterionId).HasColumnName("wcag_criterion_id");
            _ = entity.Property(e => e.WcagCriterion).HasColumnName("wcag_criterion");
            _ = entity.Property(e => e.Level)
                .HasColumnName("level")
                .HasConversion(new EnumToStringConverter<ResultLevel>())
                .HasMaxLength(15)
                .IsRequired();
            _ = entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasConversion(new EnumToStringConverter<Severity>())
                .HasMaxLength(8)
                .IsRequired();
            _ = entity.Property(e => e.Description).HasColumnName("description");
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.HasMany(r => r.Errors)
                .WithOne(e => e.Result)
                .HasForeignKey(e => e.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
            // Índices sugeridos
            _ = entity.HasIndex(e => e.AnalysisId).HasDatabaseName("idx_results_analysis");
            _ = entity.HasIndex(e => e.Severity).HasDatabaseName("idx_results_severity");
        });

        // ERRORS
        _ = modelBuilder.Entity<Error>(entity =>
        {
            _ = entity.ToTable("errors");
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.ResultId).HasColumnName("result_id");
            // Índices sugeridos
            _ = entity.HasIndex(e => e.ResultId).HasDatabaseName("idx_errors_result");
            _ = entity.Property(e => e.WcagCriterionId).HasColumnName("wcag_criterion_id");
            _ = entity.Property(e => e.ErrorCode).HasColumnName("error_code");
            _ = entity.Property(e => e.Description).HasColumnName("description");
            _ = entity.Property(e => e.Location).HasColumnName("location");
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }
}
