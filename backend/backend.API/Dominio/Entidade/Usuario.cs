using backend.API.Data;

namespace backend.API.Dominio.Entidade
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string SenhaHash { get; set; }
        public int QuantidadeFalhasLogin { get; set; }
        public DateTime DataBloqueio { get; set; }
        public ICollection<Logs> Logs { get; set; } = [];
    }
}
