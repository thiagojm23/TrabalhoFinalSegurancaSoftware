namespace backend.API
{
    public class Logs
    {
        public Guid Id { get; set; }
        public required string TelaAcao { get; set; }
        public DateTime DataAcao { get; set; } = DateTime.Now;
        public string? DescricaoAcao { get; set; }
        public required string TituloAcao { get; set; }
        public Guid UsuarioId { get; set; }

        public required Usuario Usuario { get; set; }

    }
}
