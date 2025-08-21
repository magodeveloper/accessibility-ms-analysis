using Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Analysis.Infrastructure.Data
{
    public class AnalysisDbContext : DbContext
    {
        public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options) { }

        public DbSet<Analysis.Domain.Entities.Analysis> Analyses { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Error> Errors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ANALYSIS
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
                entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
                entity.Property(e => e.WcagVersion)
                    .HasColumnName("wcag_version")
                    .HasMaxLength(3)
                    .IsRequired();
                entity.Property(e => e.WcagLevel)
                    .HasColumnName("wcag_level")
                    .HasConversion(new EnumToStringConverter<WcagLevel>())
                    .HasMaxLength(3)
                    .IsRequired();
                entity.Property(e => e.AxeViolations).HasColumnName("axe_violations").HasDefaultValue(0);
                entity.Property(e => e.AxeNeedsReview).HasColumnName("axe_needs_review").HasDefaultValue(0);
                entity.Property(e => e.AxeRecommendations).HasColumnName("axe_recommendations").HasDefaultValue(0);
                entity.Property(e => e.AxePasses).HasColumnName("axe_passes").HasDefaultValue(0);
                entity.Property(e => e.AxeIncomplete).HasColumnName("axe_incomplete").HasDefaultValue(0);
                entity.Property(e => e.AxeInapplicable).HasColumnName("axe_inapplicable").HasDefaultValue(0);
                entity.Property(e => e.EaViolations).HasColumnName("ea_violations").HasDefaultValue(0);
                entity.Property(e => e.EaNeedsReview).HasColumnName("ea_needs_review").HasDefaultValue(0);
                entity.Property(e => e.EaRecommendations).HasColumnName("ea_recommendations").HasDefaultValue(0);
                entity.Property(e => e.EaPasses).HasColumnName("ea_passes").HasDefaultValue(0);
                entity.Property(e => e.EaIncomplete).HasColumnName("ea_incomplete").HasDefaultValue(0);
                entity.Property(e => e.EaInapplicable).HasColumnName("ea_inapplicable").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.HasMany(a => a.Results).WithOne(r => r.Analysis).HasForeignKey(r => r.AnalysisId);
                // Índices sugeridos
                entity.HasIndex(e => e.UserId).HasDatabaseName("idx_analysis_user");
                entity.HasIndex(e => e.Status).HasDatabaseName("idx_analysis_status");
                entity.HasIndex(e => e.DateAnalysis).HasDatabaseName("idx_analysis_date");
            });

            // RESULTS
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
                entity.HasMany(r => r.Errors)
                    .WithOne(e => e.Result)
                    .HasForeignKey(e => e.ResultId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Índices sugeridos
                entity.HasIndex(e => e.AnalysisId).HasDatabaseName("idx_results_analysis");
                entity.HasIndex(e => e.Severity).HasDatabaseName("idx_results_severity");
            });

            // ERRORS
            modelBuilder.Entity<Error>(entity =>
            {
                entity.ToTable("ERRORS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ResultId).HasColumnName("result_id");
                // Índices sugeridos
                entity.HasIndex(e => e.ResultId).HasDatabaseName("idx_errors_result");
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