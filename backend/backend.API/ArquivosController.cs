using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.API
{
    [ApiController]
    [Route("api/TrabalhoSF/[controller]/[Action]")]
    [ValidateAntiForgeryToken]
    public class ArquivosController(LogsService logsService) : ControllerBase
    {
        private readonly LogsService _logsService = logsService;

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

            var nomeArquivoSeguro = $"{Guid.NewGuid()}{extensao}";
            var caminhoArquivo = Path.Combine(caminhoBase, nomeArquivoSeguro);

            var caminhoCompleto = Path.GetFullPath(caminhoArquivo);
            var caminhoBaseCompleto = Path.GetFullPath(caminhoBase);
            
            if (!caminhoCompleto.StartsWith(caminhoBaseCompleto))
            {
                return BadRequest("Caminho de arquivo inválido.");
            }

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
                        usuarioId:Guid.Parse(usuarioId),
                        telaAcao:"Upload",
                        tituloAcao:"Arquivo Enviado",
                        descricaoAcao:$"Arquivo {nomeArquivoSeguro} enviado com sucesso");
                }

                return Ok(new { 
                    mensagem = "Arquivo enviado com sucesso.",
                    nomeArquivo = nomeArquivoSeguro
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex}. Erro ao salvar o arquivo.");
            }
        }
    }
}
