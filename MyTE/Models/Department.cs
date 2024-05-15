using System.ComponentModel.DataAnnotations;

namespace MyTE_grupo05.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Nome do departamento é obrigatório.")]
        [StringLength(100, ErrorMessage = "Nome do departamento deve ter no máximo 100 caracteres.")]
        public string Name { get; set; }

        // Relacionamento com Tabela de Funcionarios
        // Um departamento pode ter vários Funcionários.
        public ICollection<Employee> Employees { get; set; }
    }
}
