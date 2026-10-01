using Microsoft.EntityFrameworkCore;
using Instituto.AD.Models;

namespace Instituto.AD.Data;

public class InstitutoDbContext : DbContext
{
    public InstitutoDbContext(DbContextOptions<InstitutoDbContext> options) : base(options) { }

    public DbSet<Carrera> Carreras => Set<Carrera>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<Administrador> Administradores => Set<Administrador>();
    public DbSet<Profesor> Profesores => Set<Profesor>();
    public DbSet<Formulario> Formularios => Set<Formulario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carreras
        modelBuilder.Entity<Carrera>(entity =>
        {
            entity.ToTable("Carreras");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DuracionAnios).IsRequired();
            entity.Property(e => e.Turno).HasMaxLength(50);
            entity.Property(e => e.Modalidad).HasMaxLength(50);
            entity.Property(e => e.Horario).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Activa");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        // Alumnos
        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.ToTable("Alumnos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.DNI).IsRequired();
            entity.Property(e => e.FechaNacimiento).IsRequired();
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Nacionalidad).HasMaxLength(100);
            entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.TituloSecundario).HasMaxLength(200);
            entity.Property(e => e.Turno).HasMaxLength(50);
            entity.Property(e => e.CarreraId).IsRequired();
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            
            entity.HasOne<Carrera>()
                  .WithMany()
                  .HasForeignKey(a => a.CarreraId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DNI).IsUnique();
        });

        // Administradores
        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.ToTable("Administradores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordTemp).HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Admin");
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            
            entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Activo] = 1");
        });

        // Profesores
        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.ToTable("Profesores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Telefono).HasMaxLength(50);
            entity.Property(e => e.Especialidad).HasMaxLength(100);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
            
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Formularios
        modelBuilder.Entity<Formulario>(entity =>
        {
            entity.ToTable("Formularios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Borrador");
            entity.Property(e => e.FechaApertura).IsRequired();
            entity.Property(e => e.FechaCierre).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });
    }
}