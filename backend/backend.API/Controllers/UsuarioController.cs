using backend.API.Dominio.Contrato;
using backend.API.Dominio.Entidade;
using backend.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.API.Controllers
{
    [ApiController]
    [Route("api/TrabalhoSF/[controller]/[Action]")]
    public class UsuarioController(IUsuarioRepositorio usuarioRepositorio, IConfiguration configuracao, LogsService logsService) : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
        private readonly IConfiguration _configuracao = configuracao;
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
                usuarioId: usuario.Id,
                telaAcao: "Cadastro",
                tituloAcao: "Novo Usuário",
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
                    usuarioId: usuario.Id,
                    telaAcao: "Login",
                    tituloAcao: "Tentativa Bloqueada",
                    descricaoAcao: $"Tentativa de login com usuário bloqueado. Tempo restante: {tempoRestante} minutos");

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
                        usuarioId: usuario.Id,
                        telaAcao: "Login",
                        tituloAcao: "Conta Bloqueada",
                        descricaoAcao: $"Usuário {usuario.Email} bloqueado por 30 minutos devido a múltiplas tentativas de login falhas");

                    return Unauthorized("Conta bloqueada por 30 minutos devido a múltiplas tentativas falhas.");
                }

                await _usuarioRepositorio.AtualizarUsuarioAsync(usuario);

                _logsService.RegistrarLog(
                    usuarioId: usuario.Id,
                    telaAcao: "Login",
                    tituloAcao: "Login Falhou",
                    descricaoAcao: $"Tentativa de login falhou para {usuario.Email}. Tentativas: {usuario.QuantidadeFalhasLogin}/5");

                return Unauthorized($"Credenciais inválidas. Tentativas restantes: {5 - usuario.QuantidadeFalhasLogin}");
            }

            usuario.QuantidadeFalhasLogin = 0;
            usuario.DataBloqueio = DateTime.MinValue;
            await _usuarioRepositorio.AtualizarUsuarioAsync(usuario);

            _logsService.RegistrarLog(
                usuarioId: usuario.Id,
                telaAcao: "Login",
                tituloAcao: "Login Bem-Sucedido",
                descricaoAcao: $"Usuário {usuario.Email} autenticado com sucesso");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuracao["Jwt:Key"] ?? throw new InvalidOperationException("Chave JWT não configurada"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                ]),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuracao["Jwt:Issuer"],
                Audience = _configuracao["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddHours(2)
            };

            Response.Cookies.Append("auth_token", tokenString, cookieOptions);

            return Ok(new
            {
                mensagem = "Autenticado com sucesso",
                usuarioId = usuario.Id,
                email = usuario.Email,
                token = tokenString // Retorna o token no corpo da resposta para uso no Swagger
            });
        }
    }
}
