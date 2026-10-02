using System.Drawing;
using System.Text.Json;
using NetLpr.Persistence.Sqlite.Models;
using NetLpr.Core.Values.Tracking;
using Microsoft.EntityFrameworkCore;

namespace NetLpr.Persistence.Sqlite
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<RtspSourceInfoDbModel> RtspSources { get; set; }
        public DbSet<StreamAnalysesAreaPointDbModel> AreaPoints { get; set; }
        public DbSet<TrackedInfoDbModel> TrackedInfos { get; set; }


        public SqliteDbContext()
        {
        }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=net_lpr.db");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RtspSourceInfoDbModel>(entity =>
            {
                entity.ToTable("RtspSources");
                entity.HasKey(k => k.Id);

                entity.HasMany(e => e.AnalysesArea)
                      .WithOne()
                      .HasForeignKey(p => p.SourceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StreamAnalysesAreaPointDbModel>(entity =>
            {
                entity.ToTable("StreamAnalysesAreaPoints");
                entity.HasKey(k => k.Id);
            });
            modelBuilder.Entity<TrackedInfoDbModel>(entity =>
            {
                entity.ToTable("TrackedInfos");

                entity.HasKey(k => k.TrackedId);
                entity.Property(e => e.PlateRectangle)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<RectangleF>(v, (JsonSerializerOptions)null)
                      );
                entity.Property(e => e.TrackingValues)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, List<ScoreAndValue>>>(v, (JsonSerializerOptions)null)
                      );
            });
       
            base.OnModelCreating(modelBuilder);
        }
    }
}