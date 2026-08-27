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

<<<<<<< HEAD
        // These three entities use key property names that don't match EF Core's
        // default convention (Id / <TypeName>Id), so the primary key must be
        // declared explicitly or EF throws "requires a primary key to be defined"
        // at startup.
        modelBuilder.Entity<DiseaseReport>().HasKey(d => d.ReportId);
        modelBuilder.Entity<YieldPrediction>().HasKey(y => y.PredictionId);
        modelBuilder.Entity<MarketplaceListing>().HasKey(l => l.ListingId);

=======
>>>>>>> e740759011a0a4ae6e49369223dc4e66eb29e00c
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
