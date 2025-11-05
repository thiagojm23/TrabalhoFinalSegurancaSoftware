namespace backend.API
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string SenhaHash { get; set; }
        public int QuantidaFalhasLogin { get; set; }
        public DateTime DataBloqueio { get; set; }

        public ICollection<Logs> Logs { get; set; } = [];
    }
}
