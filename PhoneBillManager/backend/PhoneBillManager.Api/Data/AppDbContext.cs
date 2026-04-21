using Microsoft.EntityFrameworkCore;
using PhoneBillManager.Api.Models;

namespace PhoneBillManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<AccountLine> AccountLines => Set<AccountLine>();
    public DbSet<PlanCharge> PlanCharges => Set<PlanCharge>();
    public DbSet<EquipmentCharge> EquipmentCharges => Set<EquipmentCharge>();
    public DbSet<ServiceCharge> ServiceCharges => Set<ServiceCharge>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("AppUsers");
            e.HasKey(u => u.UserId);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.MobileNumber).HasMaxLength(20).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            e.Property(u => u.PasswordSalt).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<Bill>(e =>
        {
            e.ToTable("Bills");
            e.HasKey(b => b.BillId);
            e.Property(b => b.TotalBillAmount).HasColumnType("decimal(10,2)");
            e.Property(b => b.TotalPlanAmount).HasColumnType("decimal(10,2)");
            e.Property(b => b.TotalEquipmentAmount).HasColumnType("decimal(10,2)");
            e.Property(b => b.TotalServicesAmount).HasColumnType("decimal(10,2)");
            e.HasOne(b => b.User).WithMany(u => u.Bills).HasForeignKey(b => b.UserId);
        });

        modelBuilder.Entity<AccountLine>(e =>
        {
            e.ToTable("AccountLines");
            e.HasKey(l => l.LineId);
            e.Property(l => l.PlanCostShare).HasColumnType("decimal(10,2)");
            e.Property(l => l.EquipmentCost).HasColumnType("decimal(10,2)");
            e.Property(l => l.ServicesCost).HasColumnType("decimal(10,2)");
            e.Property(l => l.TotalLineCost).HasColumnType("decimal(10,2)");
            e.HasOne(l => l.Bill).WithMany(b => b.AccountLines).HasForeignKey(l => l.BillId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanCharge>(e =>
        {
            e.ToTable("PlanCharges");
            e.HasKey(p => p.PlanChargeId);
            e.Property(p => p.ChargeAmount).HasColumnType("decimal(10,2)");
            e.HasOne(p => p.Bill).WithMany(b => b.PlanCharges).HasForeignKey(p => p.BillId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EquipmentCharge>(e =>
        {
            e.ToTable("EquipmentCharges");
            e.HasKey(eq => eq.EquipmentChargeId);
            e.Property(eq => eq.ChargeAmount).HasColumnType("decimal(10,2)");
            e.HasOne(eq => eq.Bill).WithMany(b => b.EquipmentCharges).HasForeignKey(eq => eq.BillId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(eq => eq.AccountLine).WithMany(l => l.EquipmentCharges).HasForeignKey(eq => eq.LineId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ServiceCharge>(e =>
        {
            e.ToTable("ServiceCharges");
            e.HasKey(sc => sc.ServiceChargeId);
            e.Property(sc => sc.ChargeAmount).HasColumnType("decimal(10,2)");
            e.HasOne(sc => sc.Bill).WithMany(b => b.ServiceCharges).HasForeignKey(sc => sc.BillId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(sc => sc.AccountLine).WithMany(l => l.ServiceCharges).HasForeignKey(sc => sc.LineId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(n => n.NotificationId);
            e.HasOne(n => n.Bill).WithMany(b => b.Notifications).HasForeignKey(n => n.BillId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(n => n.AccountLine).WithMany(l => l.Notifications).HasForeignKey(n => n.LineId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
