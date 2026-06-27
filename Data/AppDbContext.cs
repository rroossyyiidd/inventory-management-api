using Microsoft.EntityFrameworkCore;
using InventoryManagement.API.Models;

namespace InventoryManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AssetCategory> AssetCategories { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<User> Users { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Loop semua entity yang sedang di-track oleh EF Core
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Saat INSERT — set CreatedAt dan UpdatedAt
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    // Saat UPDATE — hanya update UpdatedAt
                    // CreatedAt tidak boleh berubah
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case ke semua tabel dan kolom
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Konversi nama tabel → snake_case
            // Contoh: AssetCategories → asset_categories
            entity.SetTableName(SnakeCaseNamingConvention.ToSnakeCase(entity.GetTableName()!));

            // Konversi nama kolom → snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(SnakeCaseNamingConvention.ToSnakeCase(property.GetColumnName()));
            }

            // Konversi nama foreign key → snake_case
            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(SnakeCaseNamingConvention.ToSnakeCase(key.GetConstraintName()!));
            }

            // Konversi nama index → snake_case
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(SnakeCaseNamingConvention.ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        // Global Query Filter — otomatis filter data yang sudah di-soft delete
        modelBuilder.Entity<Asset>().HasQueryFilter(a => a.DeletedAt == null);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AssetAssignment>().HasQueryFilter(aa => aa.DeletedAt == null);
        modelBuilder.Entity<MaintenanceLog>().HasQueryFilter(ml => ml.DeletedAt == null);

        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasMany(e => e.Assets)
                  .WithOne(a => a.AssetCategory)
                  .HasForeignKey(a => a.AssetCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssetCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
            entity.HasIndex(e => e.AssetCode).IsUnique();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);

            entity.HasMany(e => e.Employees)
                  .WithOne(emp => emp.Department)
                  .HasForeignKey(emp => emp.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.AssetAssignments)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                  .WithMany(emp => emp.AssetAssignments)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaintenanceLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TechnicianName).HasMaxLength(200);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.MaintenanceLogs)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<AssetCategory>().HasData(
            new AssetCategory { Id = 1, Name = "Elektronik", Description = "Laptop, monitor, printer, dll", CreatedAt = seedDate, UpdatedAt = seedDate },
            new AssetCategory { Id = 2, Name = "Furnitur", Description = "Meja, kursi, lemari, dll", CreatedAt = seedDate, UpdatedAt = seedDate },
            new AssetCategory { Id = 3, Name = "Kendaraan", Description = "Mobil, motor operasional", CreatedAt = seedDate, UpdatedAt = seedDate }
        );

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "IT", Location = "Lantai 3", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Department { Id = 2, Name = "HR", Location = "Lantai 1", CreatedAt = seedDate, UpdatedAt = seedDate },
            new Department { Id = 3, Name = "Finance", Location = "Lantai 2", CreatedAt = seedDate, UpdatedAt = seedDate }
        );
    }
}