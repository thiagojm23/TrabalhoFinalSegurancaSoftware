using System.ComponentModel.DataAnnotations;

namespace backend.API
{
    public class UsuarioContrato
    {
        [EmailAddress(ErrorMessage = "Email inválido")]
        public required string Email { get; set; }
        [MinLength(3)]
        public required string Senha { get; set; }
    }
}
