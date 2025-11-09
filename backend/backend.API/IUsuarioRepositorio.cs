namespace backend.API
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> BuscarPorIdAsync(Guid id);
        Task<Usuario?> BuscarPorEmailAsync(string email);
        Task<Usuario> CadastrarUsuarioAsync(Usuario usuario);
        Task<Usuario> AtualizarUsuarioAsync(Usuario usuario);
        string GerarHashSenha(string senha);
        bool VerificarSenha(string senha, string senhaHash);
    }
}
