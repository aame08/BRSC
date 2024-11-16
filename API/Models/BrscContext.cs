using Microsoft.EntityFrameworkCore;

namespace API.Models;

public partial class BrscContext : DbContext
{
    public BrscContext()
    {
    }

    public BrscContext(DbContextOptions<BrscContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                                ?? "server=localhost;username=root;password=ame0372;database=brsc";
            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.NameRole)
                .HasMaxLength(50)
                .HasColumnName("name_role");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdRole, "id_role");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.EmailUser)
                .HasMaxLength(100)
                .HasColumnName("email_user");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.NameUser)
                .HasMaxLength(100)
                .HasColumnName("name_user");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(60)
                .HasColumnName("password_hash");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
