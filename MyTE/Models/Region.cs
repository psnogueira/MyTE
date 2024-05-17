using System.ComponentModel.DataAnnotations;

namespace MyTE.Models
{
    public class Region
    {
        [Key]
        public int RegionId { get; set; }

        [Required(ErrorMessage = "O Nome da Região é obrigatório.")]
        [StringLength(50, ErrorMessage = "O Nome da Região deve ter no máximo 50 caracteres.")]
        public string? Name { get; set; }

        // Relação de muitos pra muitos com Tabela "Holidays"
        //
    }
}
