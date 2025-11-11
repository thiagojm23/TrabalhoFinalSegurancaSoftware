using backend.API.Data;
using backend.API.Dominio.Entidade;
using backend.API.Services.Interface;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace backend.API.Services.Repository
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
            return await _usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> CadastrarUsuarioAsync(Usuario usuario)
        {
            _usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public string GerarHashSenha(string senha)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string hash = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: senha,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                )
            );
            return $"{Convert.ToBase64String(salt)}.{hash}";

        }
        public bool VerificarSenha(string senha, string senhaHash)
        {
            var partes = senhaHash.Split('.');
            if (partes.Length != 2)
                return false;
            var salt = Convert.FromBase64String(partes[0]);
            var hash = partes[1];

            string hashVerificado = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: senha,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashVerificado;
        }
        public async Task<Usuario> AtualizarUsuarioAsync(Usuario usuario)
        {
            _usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

    }
}
