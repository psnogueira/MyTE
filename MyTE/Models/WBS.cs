using MyTE.Models.Enum;
using System.ComponentModel.DataAnnotations;


namespace MyTE.Models
{
    public class WBS
    {
        [Key]
        public int WBSId { get; set; }

        [Display(Name = "Código")]
        [Required(ErrorMessage = "Código da WBS é obrigatório.")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "Código da WBS deve ter no máximo 10 caracteres e no mínimo 4.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Apenas letras e números são permitidos.")]
        public string Code { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "Descrição da WBS é obrigatória.")]
        [StringLength(255, ErrorMessage = "Descrição da WBS deve ter no máximo 255 caracteres.")]
        public string Desc { get; set; }

        [Display(Name = "Tipo")]
        public WBSType Type { get; set; }
    }
}
