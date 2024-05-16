using System.ComponentModel.DataAnnotations;
using MyTE.Models.Enum;

namespace MyTE.Models
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


        public WBSType Type { get; set; }
    }
}
