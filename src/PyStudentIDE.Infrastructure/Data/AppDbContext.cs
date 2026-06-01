using Microsoft.EntityFrameworkCore;
using PyStudentIDE.Domain.Entities;

namespace PyStudentIDE.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<Entrega> Entregas => Set<Entrega>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<ValidacionHash> ValidacionesHash => Set<ValidacionHash>();
    public DbSet<CommitGit> CommitsGit => Set<CommitGit>();
    public DbSet<CasoPrueba> CasosPrueba => Set<CasoPrueba>();
    public DbSet<ResultadoPrueba> ResultadosPrueba => Set<ResultadoPrueba>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("ROL");
            e.HasKey(r => r.IdRol);
            e.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
            e.HasIndex(r => r.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("USUARIO");
            e.HasKey(u => u.IdUsuario);
            e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
            e.Property(u => u.Correo).HasMaxLength(150).IsRequired();
            e.HasIndex(u => u.Correo).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Curso>(e =>
        {
            e.ToTable("CURSO");
            e.HasKey(c => c.IdCurso);
            e.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
            e.HasIndex(c => c.Codigo).IsUnique();
            e.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            e.Property(c => c.Periodo).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Matricula>(e =>
        {
            e.ToTable("MATRICULA");
            e.HasKey(m => m.IdMatricula);
            e.Property(m => m.TipoParticipacion).HasMaxLength(20).IsRequired();
            e.HasIndex(m => new { m.IdUsuario, m.IdCurso }).IsUnique();
        });

        modelBuilder.Entity<Asignacion>(e =>
        {
            e.ToTable("ASIGNACION");
            e.HasKey(a => a.IdAsignacion);
            e.Property(a => a.Titulo).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Entrega>(e =>
        {
            e.ToTable("ENTREGA");
            e.HasKey(en => en.IdEntrega);
            e.Property(en => en.Estado).HasMaxLength(30).IsRequired();
            e.Property(en => en.Calificacion).HasColumnType("DECIMAL(5,2)");
        });

        modelBuilder.Entity<Archivo>(e =>
        {
            e.ToTable("ARCHIVO");
            e.HasKey(a => a.IdArchivo);
            e.Property(a => a.NombreArchivo).HasMaxLength(150).IsRequired();
            e.Property(a => a.TipoArchivo).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<ValidacionHash>(e =>
        {
            e.ToTable("VALIDACION_HASH");
            e.HasKey(v => v.IdValidacion);
            e.Property(v => v.Algoritmo).HasMaxLength(20).IsRequired();
            e.Property(v => v.HashCalculado).HasMaxLength(64).IsRequired();
            e.HasIndex(v => v.IdArchivo).IsUnique();
        });

        modelBuilder.Entity<CommitGit>(e =>
        {
            e.ToTable("COMMIT_GIT");
            e.HasKey(c => c.IdCommit);
            e.Property(c => c.HashCommit).HasMaxLength(40).IsRequired();
            e.HasIndex(c => c.HashCommit).IsUnique();
            e.Property(c => c.Rama).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CasoPrueba>(e =>
        {
            e.ToTable("CASO_PRUEBA");
            e.HasKey(c => c.IdCasoPrueba);
            e.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ResultadoPrueba>(e =>
        {
            e.ToTable("RESULTADO_PRUEBA");
            e.HasKey(r => r.IdResultado);
            e.Property(r => r.TiempoEjecucion).HasColumnType("DECIMAL(8,3)");
            e.HasIndex(r => new { r.IdEntrega, r.IdCasoPrueba }).IsUnique();
        });
    }
}
