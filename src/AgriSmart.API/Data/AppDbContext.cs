using AgriSmart.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriSmart.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<CropRecord> CropRecords => Set<CropRecord>();
    public DbSet<SoilData> SoilData => Set<SoilData>();
    public DbSet<DiseaseReport> DiseaseReports => Set<DiseaseReport>();
    public DbSet<YieldPrediction> YieldPredictions => Set<YieldPrediction>();
    public DbSet<MarketplaceListing> MarketplaceListings => Set<MarketplaceListing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Farm>()
            .HasOne(f => f.User)
            .WithMany(u => u.Farms)
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<CropRecord>()
            .HasOne(c => c.Farm)
            .WithMany(f => f.CropRecords)
            .HasForeignKey(c => c.FarmId);

        modelBuilder.Entity<SoilData>()
            .HasOne(s => s.Farm)
            .WithMany(f => f.SoilData)
            .HasForeignKey(s => s.FarmId);

        modelBuilder.Entity<DiseaseReport>()
            .HasOne(d => d.CropRecord)
            .WithMany(c => c.DiseaseReports)
            .HasForeignKey(d => d.CropRecordId);

        modelBuilder.Entity<YieldPrediction>()
            .HasOne(y => y.CropRecord)
            .WithMany(c => c.YieldPredictions)
            .HasForeignKey(y => y.CropRecordId);

        modelBuilder.Entity<MarketplaceListing>()
            .HasOne(l => l.User)
            .WithMany(u => u.Listings)
            .HasForeignKey(l => l.UserId);

        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
    }
}
