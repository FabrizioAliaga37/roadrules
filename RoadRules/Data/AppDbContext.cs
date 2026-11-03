using Microsoft.EntityFrameworkCore;

namespace RoadRules.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Jurisdiction> Jurisdictions => Set<Jurisdiction>();
    public DbSet<AxleLimit> AxleLimits => Set<AxleLimit>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Jurisdiction>().HasIndex(j => j.Code).IsUnique();

        b.Entity<Jurisdiction>().HasData(
            new Jurisdiction { Id = 1, Code = "ON", Name = "Ontario" },
            new Jurisdiction { Id = 2, Code = "QC", Name = "Quebec" },
            new Jurisdiction { Id = 3, Code = "AB", Name = "Alberta" },
            new Jurisdiction { Id = 4, Code = "BC", Name = "British Columbia" },
            new Jurisdiction { Id = 5, Code = "MB", Name = "Manitoba" });

        // Illustrative numbers only. Not real limits.
        var start = new DateOnly(2026, 1, 1);
        var id = 1;
        var rows = new List<AxleLimit>();
        foreach (var (jid, single, tandem, tridem) in new[]
        {
            (1, 9000, 17000, 24000),
            (2, 9000, 18000, 26000),
            (3, 9100, 17000, 24000),
            (4, 9100, 17000, 24000),
            (5, 9100, 17000, 23000)
        })
        {
            rows.Add(new AxleLimit { Id = id++, JurisdictionId = jid, AxleGroup = "Single", AxleCount = 1, MaxKg = single, EffectiveFrom = start });
            rows.Add(new AxleLimit { Id = id++, JurisdictionId = jid, AxleGroup = "Tandem", AxleCount = 2, MaxKg = tandem, EffectiveFrom = start });
            rows.Add(new AxleLimit { Id = id++, JurisdictionId = jid, AxleGroup = "Tridem", AxleCount = 3, MaxKg = tridem, EffectiveFrom = start });
        }
        b.Entity<AxleLimit>().HasData(rows);
    }
}
