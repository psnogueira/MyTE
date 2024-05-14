using System.ComponentModel.DataAnnotations;

namespace MyTE_grupo05.Models
{
    public class Holiday
    {
        [Key]
        public int HolidayId { get; set; }

        [Required(ErrorMessage = "O Nome do Feriado é obrigatório.")]
        [StringLength(50, ErrorMessage = "O Nome do Feriado deve ter no máximo 50 caracteres.")]
        public string? Name { get; set; }

        // Relação de muitos pra muitos com Tabela "Regions"
        // 
    }
}
