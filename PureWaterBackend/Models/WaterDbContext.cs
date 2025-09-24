using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PureWaterBackend.Models;

public partial class WaterDbContext : DbContext
{
    public WaterDbContext()
    {
    }

    public WaterDbContext(DbContextOptions<WaterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Water> Water { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07277324C6");

            entity.HasIndex(e => e.GoogleId, "UQ__Users__A6FBF2FB508C5773").IsUnique();

            entity.Property(e => e.DailyNormMl).HasDefaultValue(2000);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.GoogleId).HasMaxLength(200);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Water>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Water__3214EC0716EE830B");

            entity.Property(e => e.AmountMl).HasColumnName("AmountML");

            entity.HasOne(d => d.User).WithMany(p => p.Water)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Water__UserId__3B75D760");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
