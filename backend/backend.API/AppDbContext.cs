using Microsoft.EntityFrameworkCore;

namespace backend.API
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Logs> Logs => Set<Logs>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entidade =>
            {
                entidade.Property(e => e.Email).HasMaxLength(50);
                entidade.HasIndex(e => e.Email).IsUnique();
                entidade.Property(e => e.SenhaHash).HasMaxLength(255);
            });

            modelBuilder.Entity<Logs>(entidade =>
            {
                entidade.Property(e => e.TelaAcao).HasMaxLength(50);
                entidade.Property(e => e.DescricaoAcao).HasMaxLength(500);
                entidade.Property(e => e.TituloAcao).HasMaxLength(50);
            });
        }
    }
}
