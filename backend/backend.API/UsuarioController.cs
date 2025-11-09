using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.API
{
    [ApiController]
    [Route("api/TrabalhoSF/[controller]/[Action]")]
    public class UsuarioController(IUsuarioRepositorio usuarioRepositorio, LogsService logsService) : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
        private readonly LogsService _logsService = logsService;

        [HttpPost]
        public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioContrato contrato)
        {
            var usuarioExistente = await _usuarioRepositorio.BuscarPorEmailAsync(contrato.Email);

            if (usuarioExistente != null)
                return BadRequest("Usuário já cadastrado");

            var usuario = new Usuario
            {
                Email = contrato.Email,
                SenhaHash = _usuarioRepositorio.GerarHashSenha(contrato.Senha),
                QuantidadeFalhasLogin = 0,
                DataBloqueio = DateTime.MinValue
            };

            await _usuarioRepositorio.CadastrarUsuarioAsync(usuario);

            _logsService.RegistrarLog(
                usuarioId:usuario.Id, 
                telaAcao:"Cadastro",
                tituloAcao:"Novo Usuário", 
                descricaoAcao: $"Usuário {usuario.Email} cadastrado com sucesso");

            return Ok(new { mensagem = "Usuário cadastrado com sucesso" });
        }

        [HttpPost]
        public async Task<IActionResult> Logar([FromBody] UsuarioContrato contrato)
        {
            var usuario = await _usuarioRepositorio.BuscarPorEmailAsync(contrato.Email);

            if (usuario == null)
            {
                return Unauthorized("Credenciais inválidas"); // ideia é deixar implícito que o usuario não existe
            }

            if (usuario.DataBloqueio > DateTime.UtcNow)
            {
                var tempoRestante = (usuario.DataBloqueio - DateTime.UtcNow).Minutes + 1;
                
                _logsService.RegistrarLog(
                    usuarioId:usuario.Id,
                    telaAcao:"Login",
                    tituloAcao:"Tentativa Bloqueada",
                    descricaoAcao:$"Tentativa de login com usuário bloqueado. Tempo restante: {tempoRestante} minutos");
                
                return Unauthorized($"Usuário bloqueado. Tente novamente em {tempoRestante} minutos.");
            }

            if (!_usuarioRepositorio.VerificarSenha(contrato.Senha, usuario.SenhaHash))
            {
                usuario.QuantidadeFalhasLogin++;
                
                if (usuario.QuantidadeFalhasLogin >= 5) // Bloquear após 5 tentativas falhas
                {
                    usuario.DataBloqueio = DateTime.UtcNow.AddMinutes(30);
                    await _usuarioRepositorio.AtualizarUsuarioAsync(usuario);
                    
                    _logsService.RegistrarLog(
                        usuarioId:usuario.Id,
                        telaAcao:"Login",
                        tituloAcao:"Conta Bloqueada", 
                        descricaoAcao:$"Usuário {usuario.Email} bloqueado por 30 minutos devido a múltiplas tentativas de login falhas");
                    
                    return Unauthorized("Conta bloqueada por 30 minutos devido a múltiplas tentativas falhas.");
                }

                await _usuarioRepositorio.AtualizarUsuarioAsync(usuario);
                
                _logsService.RegistrarLog(
                    usuarioId:usuario.Id,
                    telaAcao:"Login",
                    tituloAcao:"Login Falhou",
                    descricaoAcao:$"Tentativa de login falhou para {usuario.Email}. Tentativas: {usuario.QuantidadeFalhasLogin}/5");
                
                return Unauthorized($"Credenciais inválidas. Tentativas restantes: {5 - usuario.QuantidadeFalhasLogin}");
            }

            usuario.QuantidadeFalhasLogin = 0;
            usuario.DataBloqueio = DateTime.MinValue;
            await _usuarioRepositorio.AtualizarUsuarioAsync(usuario);

            _logsService.RegistrarLog(
                usuarioId:usuario.Id, 
                telaAcao:"Login",
                tituloAcao:"Login Bem-Sucedido",
                descricaoAcao:$"Usuário {usuario.Email} autenticado com sucesso");

            return Ok(new 
            { 
                mensagem = "Autenticado com sucesso",
                usuarioId = usuario.Id,
                email = usuario.Email
            });
        }
    }
}
