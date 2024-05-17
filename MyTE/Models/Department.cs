using System.ComponentModel.DataAnnotations;

namespace MyTE.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Nome do departamento é obrigatório.")]
        [StringLength(100, ErrorMessage = "Nome do departamento deve ter no máximo 100 caracteres.")]
        public string? Name { get; set; }

        //Relacionamento com a tabela de usuários (Um departamento tem vários usuários/funcionários)
        public virtual ICollection<ApplicationUser>? Users { get; set; }
    }
}
