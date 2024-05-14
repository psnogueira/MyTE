using System.ComponentModel.DataAnnotations;

namespace MyTE_grupo05.Models
{
    public class Login
    {
        [Key]
        public int LoginId { get; set; }

        [Required(ErrorMessage = "Login é obrigatório.")]
        [StringLength(50, ErrorMessage = "Login deve ter no máximo 50 caracteres.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [StringLength(50, ErrorMessage = "Senha deve ter no máximo 50 caracteres.")]
        public string? Senha { get; set; }
    }
}
