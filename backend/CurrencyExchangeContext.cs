using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

public class CurrencyExchangeContext : DbContext
{
    public DbSet<TableA> Tables { get; set; }
    public DbSet<Rate> Rates { get; set; }

    public CurrencyExchangeContext(DbContextOptions<CurrencyExchangeContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja tabeli TableA
        modelBuilder.Entity<TableA>()
            .ToTable("TableA")
            .HasKey(t => t.Id);

        modelBuilder.Entity<TableA>()
            .Property(t => t.TableName)
            .IsRequired();

        modelBuilder.Entity<TableA>()
            .Property(t => t.No)
            .IsRequired();

        modelBuilder.Entity<TableA>()
            .Property(t => t.EffectiveDate)
            .IsRequired();

        // Konfiguracja tabeli Rates
        modelBuilder.Entity<Rate>()
            .ToTable("Rates")
            .HasKey(r => r.Id);

        modelBuilder.Entity<Rate>()
            .Property(r => r.Currency)
            .IsRequired();

        modelBuilder.Entity<Rate>()
            .Property(r => r.Code)
            .IsRequired();

        modelBuilder.Entity<Rate>()
            .Property(r => r.Mid)
            .IsRequired();

        modelBuilder.Entity<Rate>()
            .HasOne(r => r.Table)
            .WithMany(t => t.Rates)
            .HasForeignKey(r => r.TableId);

        // Unikalność daty i kodu w tabeli Rates
        modelBuilder.Entity<Rate>()
            .HasIndex(r => new { r.TableId, r.Code })
            .IsUnique();
    }
}

// Tabela główna dla informacji o tabelach
public class TableA
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TableName { get; set; }

    [Required]
    public string No { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    public ICollection<Rate> Rates { get; set; }
}

// Tabela dla kursów walut
public class Rate
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TableId { get; set; }

    [Required]
    public string Currency { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    public decimal Mid { get; set; }

    public TableA Table { get; set; }
}

