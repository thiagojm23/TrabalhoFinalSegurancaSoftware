namespace backend.API
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> BuscarPorIdAsync(Guid id);
        Task<Usuario?> BuscarPorNomeAsync(string email);
        Task<Usuario> CadastrarUsuarioAsync(Usuario usuario);
    }
}
