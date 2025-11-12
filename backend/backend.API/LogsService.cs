using System.Net;
using backend.API;

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
        };

        _context.Logs.Add(log);
        _context.SaveChanges();
    }

    public string NeutralizaEntradaUsuario(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove caracteres de controle perigosos (quebras de linha e tabulações)
        input = input.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");

        // HTML Encode para prevenir XSS
        // Converte caracteres especiais HTML como <, >, &, ", ' para entidades HTML seguras
        input = WebUtility.HtmlEncode(input);

        // Limita o tamanho para evitar ataques de negação de serviço e respeitar limites do banco
        if (input.Length > 500)
            input = input[..500];

        return input.Trim();
    }
}