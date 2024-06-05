//Essa classe foi criada para subtituir a classe IdentityUser adicionando novas colunas para a tabela de usuários no banco de dados. Ela estende a classe IdentityUser. É a base para o cadastro de usuários, login e autenticação com Identity. 

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTE.Models
{
    public class ApplicationUser : IdentityUser

    {
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }

        [Display(Name = "Sobrenome")]
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data de Contratação")]
        public DateTime HiringDate { get; set; }

        public string PID { get; set; }


        [ForeignKey("Department")]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        [Display(Name = "Nível de Acesso")]
        public string? RoleId { get; set; }


        [NotMapped]
        [Display(Name = "Nome Completo")]
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }

    }
}
