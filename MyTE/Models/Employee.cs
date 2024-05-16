using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyTE.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Nome do funcionário é obrigatório.")]
        [StringLength(100, ErrorMessage = "Nome do funcionário deve ter no máximo 100 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Data de contratação é obrigatória.")]
        [DataType(DataType.Date, ErrorMessage = "Data de contratação deve ser uma data válida.")]
        public DateTime StartDate { get; set; }

       /**
       // Relacionamento com Tabela de Niveis de Acesso
       // Um Funcionário pode ter apenas um nível de acesso.
       [Required(ErrorMessage = "Nível de acesso é obrigatório.")]
       [ForeignKey(nameof(AccessLevel))]
       public int LevelId { get; set; }
       public AccessLevel AccessLevel { get; set; }
        */

        // Relacionamento com Tabela de Departamentos
        // Um Funcionário pode estar em apenas um departamento.
        [Required(ErrorMessage = "Departamento é obrigatório.")]
        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
