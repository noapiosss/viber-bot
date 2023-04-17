using Contracts.Database;
using Microsoft.EntityFrameworkCore;

namespace Domain.Database
{
    public partial class TrackDbContext : DbContext
    {
        public TrackDbContext()
        {
        }

        public TrackDbContext(DbContextOptions<TrackDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TrackLocation> TrackLocations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = optionsBuilder.UseSqlServer("Server=localhost;Database=Track;User=noapioss;Password=123456;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TrackLocation>(entity =>
            {
                _ = entity.HasKey(e => e.Id).HasName("PK_TackLocation_id");

                _ = entity.ToTable("TrackLocation");

                _ = entity.Property(e => e.Id).HasColumnName("id");
                _ = entity.Property(e => e.DateEvent).HasColumnType("datetime");
                _ = entity.Property(e => e.DateTrack)
                    .HasColumnType("datetime")
                    .HasColumnName("date_track");
                _ = entity.Property(e => e.Imei)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IMEI");
                _ = entity.Property(e => e.Latitude)
                    .HasColumnType("decimal(12, 9)")
                    .HasColumnName("latitude");
                _ = entity.Property(e => e.Longitude)
                    .HasColumnType("decimal(12, 9)")
                    .HasColumnName("longitude");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}