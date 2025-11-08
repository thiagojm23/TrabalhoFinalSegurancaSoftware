using Microsoft.AspNetCore.Mvc;

namespace backend.API
{
    [ApiController]
    [Route("api/TrabalhoSF/[controller]/[Action]")]
    public class UsuarioController(IUsuarioRepositorio usuarioRepositorio) : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;

        [HttpPost]
        public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioContrato contrato)
        {
            var usuario = await _usuarioRepositorio.BuscarPorNomeAsync(contrato.Email);

            if (usuario != null)
                return BadRequest("Usuário já cadastrado");

            var entidade = new Usuario
            {
                Email = contrato.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(contrato.Senha)
            };

            await _usuarioRepositorio.CadastrarUsuarioAsync(entidade);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Logar([FromBody] UsuarioContrato contrato)
        {
            var usuario = await _usuarioRepositorio.BuscarPorNomeAsync(contrato.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrato.Senha, usuario.SenhaHash))
                return BadRequest("Credenciais inválidas");

            var entidade = new Usuario
            {
                Email = contrato.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(contrato.Senha)
            };

            await _usuarioRepositorio.CadastrarUsuarioAsync(entidade);

            return Ok();
        }
    }
}
