using DataGo.Application;
using DataGo.Domain;
using Microsoft.EntityFrameworkCore;

namespace DataGo.Infrastructure;

public sealed class DataGoDbContext(DbContextOptions<DataGoDbContext> options) : DbContext(options)
{
    public DbSet<ResidentialCustomer> ResidentialCustomers => Set<ResidentialCustomer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<TransportZone> TransportZones => Set<TransportZone>();
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();
    public DbSet<Center> Centers => Set<Center>();
    public DbSet<ModernChannelCustomer> ModernChannelCustomers => Set<ModernChannelCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ResidentialCustomer>(entity =>
        {
            entity.ToTable("residential_customers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.DocumentType, x.DocumentNumber }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(20);
            entity.Property(x => x.BusinessName).HasMaxLength(200);
            entity.Property(x => x.ExtendedLegalName).HasMaxLength(250);
            entity.Property(x => x.FullName).HasMaxLength(250);
            entity.Property(x => x.FirstNames).HasMaxLength(250);
            entity.Property(x => x.LastNames).HasMaxLength(250);
            entity.Property(x => x.DocumentNumber).HasMaxLength(30);
            entity.Property(x => x.PaymentCondition).HasMaxLength(30);
            entity.HasOne(x => x.Address).WithOne().HasForeignKey<CustomerAddress>(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Center>().WithMany().HasForeignKey(x => x.CenterId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.ToTable("customer_addresses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FormattedAddress).HasMaxLength(300);
            entity.HasOne<Neighborhood>().WithMany().HasForeignKey(x => x.NeighborhoodId).OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureCatalogs(modelBuilder);
        SeedData.Add(modelBuilder);
    }

    private static void ConfigureCatalogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>().ToTable("countries");
        modelBuilder.Entity<Department>().ToTable("departments");
        modelBuilder.Entity<Department>().HasOne<Country>().WithMany().HasForeignKey(x => x.CountryId);
        modelBuilder.Entity<Municipality>().ToTable("municipalities");
        modelBuilder.Entity<Municipality>().HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId);
        modelBuilder.Entity<TransportZone>(entity => { entity.ToTable("transport_zones"); entity.HasIndex(x => x.Code).IsUnique(); });
        modelBuilder.Entity<Neighborhood>(entity =>
        {
            entity.ToTable("neighborhoods");
            entity.HasIndex(x => x.Name);
            entity.HasOne<Municipality>().WithMany().HasForeignKey(x => x.MunicipalityId);
            entity.HasOne<TransportZone>().WithMany().HasForeignKey(x => x.TransportZoneId);
        });
        modelBuilder.Entity<Center>(entity => { entity.ToTable("centers"); entity.HasIndex(x => x.Code).IsUnique(); });
        modelBuilder.Entity<ModernChannelCustomer>(entity =>
        {
            entity.ToTable("modern_channel_customers");
            entity.HasIndex(x => new { x.DocumentType, x.DocumentNumber }).IsUnique();
        });
    }
}
