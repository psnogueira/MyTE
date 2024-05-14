using System.ComponentModel.DataAnnotations;
using MyTE_grupo05.Models.Enum;

namespace MyTE_grupo05.Models
{
    public class WBS
    {
        [Key]
        public int WBSId { get; set; }

        [Required(ErrorMessage = "Código da WBS é obrigatório.")]
        [StringLength(16, ErrorMessage = "Código da WBS deve ter no máximo 10 caracteres.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Descrição da WBS é obrigatória.")]
        [StringLength(255, ErrorMessage = "Descrição da WBS deve ter no máximo 255 caracteres.")]
        public string Desc { get; set; }

        [Required(ErrorMessage = "O tipo da WBS é obrigatório.")]
        public WBSType Type { get; set; }
    }
}
