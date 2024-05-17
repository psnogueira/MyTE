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

        // Relacionamento com Tabela de Funcionarios
        // Um departamento pode ter vários Funcionários.
        public virtual ICollection<Employee>? Employees { get; set; }
    }
}
