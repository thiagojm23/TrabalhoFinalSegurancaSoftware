using System.ComponentModel.DataAnnotations;

namespace backend.API.Dominio.Contrato
{
    public class UsuarioContrato
    {
        [Required(ErrorMessage = "O campo email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do email é inválido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "O campo senha é obrigatório.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
        public required string Senha { get; set; }
    }
}
