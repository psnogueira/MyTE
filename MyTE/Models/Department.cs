using System.ComponentModel.DataAnnotations;

namespace MyTE.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "Nome do departamento é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do departamento deve ter no máximo 100 caracteres.")]
        public string? Name { get; set; }

        //Relacionamento com a tabela de usuários (Um departamento tem vários usuários/funcionários) - Reparar que foi modificado de Employees para Users (ApplicationUser) pois a classe Employee não existe mais 
        public virtual ICollection<ApplicationUser>? Users { get; set; }
    }
}
