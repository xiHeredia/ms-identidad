using Microsoft.EntityFrameworkCore;
using MsIdentidad.Api.Data.Entities;

namespace MsIdentidad.Api.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
    public DbSet<RolEntity> Roles => Set<RolEntity>();
    public DbSet<UsuarioRolEntity> UsuarioRoles => Set<UsuarioRolEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioEntity>(builder =>
        {
            builder.ToTable("usuario");
            builder.HasKey(x => x.UsuId);
            builder.Property(x => x.UsuId).HasColumnName("usu_id").ValueGeneratedOnAdd();
            builder.Property(x => x.UsuGuid).HasColumnName("usu_guid").IsRequired();
            builder.Property(x => x.UsuLogin).HasColumnName("usu_login").HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuPasswordHash).HasColumnName("usu_password_hash").HasMaxLength(256).IsRequired();
            builder.Property(x => x.UsuFechaRegistro).HasColumnName("usu_fecha_registro").IsRequired();
            builder.Property(x => x.UsuUsuarioRegistro).HasColumnName("usu_usuario_registro").HasMaxLength(100).IsRequired();
            builder.Property(x => x.UsuIpRegistro).HasColumnName("usu_ip_registro").HasMaxLength(45).IsRequired();
            builder.Property(x => x.UsuFechaMod).HasColumnName("usu_fecha_mod");
            builder.Property(x => x.UsuUsuarioMod).HasColumnName("usu_usuario_mod").HasMaxLength(100);
            builder.Property(x => x.UsuIpMod).HasColumnName("usu_ip_mod").HasMaxLength(45);
            builder.Property(x => x.UsuFechaEliminacion).HasColumnName("usu_fecha_eliminacion");
            builder.Property(x => x.UsuUsuarioEliminacion).HasColumnName("usu_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.UsuIpEliminacion).HasColumnName("usu_ip_eliminacion").HasMaxLength(45);
            builder.Property(x => x.UsuEstado).HasColumnName("usu_estado").HasMaxLength(1).IsRequired();
            builder.HasIndex(x => x.UsuGuid).IsUnique();
            builder.HasIndex(x => x.UsuLogin).IsUnique();
        });

        modelBuilder.Entity<RolEntity>(builder =>
        {
            builder.ToTable("roles");
            builder.HasKey(x => x.RolId);
            builder.Property(x => x.RolId).HasColumnName("rol_id").ValueGeneratedOnAdd();
            builder.Property(x => x.RolGuid).HasColumnName("rol_guid").IsRequired();
            builder.Property(x => x.RolDescripcion).HasColumnName("rol_descripcion").HasMaxLength(80).IsRequired();
            builder.Property(x => x.RolFechaIngreso).HasColumnName("rol_fecha_ingreso").IsRequired();
            builder.Property(x => x.RolUsuarioIngreso).HasColumnName("rol_usuario_ingreso").HasMaxLength(100).IsRequired();
            builder.Property(x => x.RolIpIngreso).HasColumnName("rol_ip_ingreso").HasMaxLength(45).IsRequired();
            builder.Property(x => x.RolFechaEliminacion).HasColumnName("rol_fecha_eliminacion");
            builder.Property(x => x.RolUsuarioEliminacion).HasColumnName("rol_usuario_eliminacion").HasMaxLength(100);
            builder.Property(x => x.RolIpEliminacion).HasColumnName("rol_ip_eliminacion").HasMaxLength(45);
            builder.Property(x => x.RolEstado).HasColumnName("rol_estado").HasMaxLength(1).IsRequired();
            builder.HasIndex(x => x.RolGuid).IsUnique();
        });

        modelBuilder.Entity<UsuarioRolEntity>(builder =>
        {
            builder.ToTable("usuarioxroles");
            builder.HasKey(x => x.UsuRolId);
            builder.Property(x => x.UsuRolId).HasColumnName("usu_rol_id").ValueGeneratedOnAdd();
            builder.Property(x => x.UsuId).HasColumnName("usu_id").IsRequired();
            builder.Property(x => x.RolId).HasColumnName("rol_id").IsRequired();
            builder.Property(x => x.UsuRolEstado).HasColumnName("usu_rol_estado").HasMaxLength(1).IsRequired();
            builder.HasIndex(x => new { x.UsuId, x.RolId }).IsUnique();
            builder.HasOne(x => x.Usuario).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.UsuId);
            builder.HasOne(x => x.Rol).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.RolId);
        });
    }
}
