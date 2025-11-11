using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.API
{
    [ApiController]
    [Route("api/TrabalhoSF/[controller]/[Action]")]
    [Authorize]
    public class ArquivosController(LogsService logsService, CriptografiaService criptografiaService) : ControllerBase
    {
        private readonly LogsService _logsService = logsService;
        private readonly CriptografiaService _criptografiaService = criptografiaService;

        [HttpPost]
        public async Task<IActionResult> UploadArquivo(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest("Nenhum arquivo foi enviado.");

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".txt", ".docx" };
            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
                return BadRequest("Tipo de arquivo não permitido.");

            if (arquivo.Length > 5 * 1024 * 1024) // 5MB
                return BadRequest("Arquivo muito grande. Tamanho máximo: 5MB.");

            var caminhoBase = Path.Combine(Directory.GetCurrentDirectory(), "Arquivos");

            if (!Directory.Exists(caminhoBase))
                Directory.CreateDirectory(caminhoBase);

            // Criptografar o nome completo do arquivo (com extensão)
            var nomeCriptografado = _criptografiaService.CriptografarNomeArquivo(arquivo.FileName);
            var nomeArquivoSeguro = $"{nomeCriptografado}{extensao}";
            var caminhoArquivo = Path.Combine(caminhoBase, nomeArquivoSeguro);

            // Verificar se o arquivo já existe
            if (System.IO.File.Exists(caminhoArquivo))
                return Conflict($"Já existe um arquivo com o nome '{arquivo.FileName}'. Por favor renomeie o arquivo antes de fazer o upload.");

            var caminhoCompleto = Path.GetFullPath(caminhoArquivo);
            var caminhoBaseCompleto = Path.GetFullPath(caminhoBase);

            if (!caminhoCompleto.StartsWith(caminhoBaseCompleto))
                return BadRequest("Caminho de arquivo inválido.");

            try
            {
                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(usuarioId))
                {
                    _logsService.RegistrarLog(
                        usuarioId: Guid.Parse(usuarioId),
                        telaAcao: "Upload",
                        tituloAcao: "Arquivo Enviado",
                        descricaoAcao: $"Arquivo {arquivo.FileName} enviado com sucesso");
                }

                return Ok(new
                {
                    mensagem = "Arquivo enviado com sucesso.",
                    nomeArquivo = arquivo.FileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar o arquivo: {ex.Message}");
            }
        }

        [HttpGet("{nomeArquivo}")]
        public IActionResult BaixarArquivo(string nomeArquivo)
        {
            if (string.IsNullOrWhiteSpace(nomeArquivo))
                return BadRequest("Nome do arquivo é obrigatório.");

            try
            {
                var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();

                // Criptografar o nome do arquivo para localizar no disco
                var nomeCriptografado = _criptografiaService.CriptografarNomeArquivo(nomeArquivo);
                var nomeArquivoSeguro = $"{nomeCriptografado}{extensao}";

                var caminhoBase = Path.Combine(Directory.GetCurrentDirectory(), "Arquivos");
                var caminhoArquivo = Path.Combine(caminhoBase, nomeArquivoSeguro);
                var caminhoCompleto = Path.GetFullPath(caminhoArquivo);
                var caminhoBaseCompleto = Path.GetFullPath(caminhoBase);

                // Validação de path traversal
                if (!caminhoCompleto.StartsWith(caminhoBaseCompleto))
                    return BadRequest("Caminho de arquivo inválido.");

                if (!System.IO.File.Exists(caminhoArquivo))
                    return NotFound("Arquivo não encontrado.");

                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(usuarioId))
                {
                    _logsService.RegistrarLog(
                        usuarioId: Guid.Parse(usuarioId),
                        telaAcao: "Download",
                        tituloAcao: "Arquivo Baixado",
                        descricaoAcao: $"Arquivo {nomeArquivo} baixado com sucesso");
                }

                var memoria = new MemoryStream();
                using (var stream = new FileStream(caminhoArquivo, FileMode.Open))
                {
                    stream.CopyTo(memoria);
                }
                memoria.Position = 0;

                var contentType = extensao switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".txt" => "text/plain",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                return File(memoria, contentType, nomeArquivo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao baixar o arquivo: {ex.Message}");
            }
        }
    }
}
