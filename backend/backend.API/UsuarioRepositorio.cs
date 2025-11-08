using Microsoft.EntityFrameworkCore;

namespace backend.API
{
    public class UsuarioRepositorio(AppDbContext appDbContext) : IUsuarioRepositorio
    {
        private readonly AppDbContext _context = appDbContext;
        private readonly DbSet<Usuario> _usuarios = appDbContext.Usuarios;

        public async Task<Usuario?> BuscarPorIdAsync(Guid id)
        {
            return await _usuarios.FindAsync(id);
        }

        public async Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            return await _usuarios.FindAsync(email);
        }

        public async Task<Usuario> CadastrarUsuarioAsync(Usuario usuario)
        {
            _usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
    }
}
