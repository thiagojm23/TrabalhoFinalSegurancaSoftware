using backend.API;
using backend.API.Data;

public class LogsService
{
    private readonly AppDbContext _context;

    public LogsService(AppDbContext context)
    {
        _context = context;
    }

    public void RegistrarLog(Guid usuarioId, string telaAcao, string tituloAcao, string descricaoAcao)
    {
        var log = new Logs
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            TelaAcao = NeutralizaEntradaUsuario(telaAcao),
            TituloAcao = NeutralizaEntradaUsuario(tituloAcao),
            DescricaoAcao = NeutralizaEntradaUsuario(descricaoAcao),
            DataAcao = DateTime.UtcNow,
            Usuario = default
        };

        _context.Logs.Add(log);
        _context.SaveChanges();
    }

    public string NeutralizaEntradaUsuario(string input)
    {
        return input.Replace("\n", "").Replace("\r", "").Replace("\t", "");
    }
}