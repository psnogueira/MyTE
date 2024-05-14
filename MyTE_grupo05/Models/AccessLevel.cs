using System.ComponentModel.DataAnnotations;

namespace MyTE_grupo05.Models
{
    public class AccessLevel
    {
        [Key]
        public int LevelId { get; set; }

        // Relacionamento com Tabela de Funcionários
        // Um mesmo Nível de Acesso pode estar em vários Funcionários.
        [Required(ErrorMessage = "Nome do nível de acesso é obrigatório.")]
        [StringLength(50, ErrorMessage = "Nome do nível de acesso deve ter no máximo 50 caracteres.")]
        public string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
