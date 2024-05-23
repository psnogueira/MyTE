﻿//Essa classe foi criada para subtituir a classe IdentityUser adicionando novas colunas para a tabela de usuários no banco de dados. Ela estende a classe IdentityUser. É a base para o cadastro de usuários, login e autenticação com Identity. 

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTE.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage ="O nome do funcionário é obrigatório.")]
        [Display(Name = "Nome")]
        [StringLength(50, ErrorMessage = "O nome do funcionário deve ter no máximo 50 caracteres.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "O sobrenome do funcionário é obrigatório")]
        [MaxLength(50, ErrorMessage = "O sobrenome do funcionário deve ter até 50 caracteres")]
        [Display(Name = "Sobrenome")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "A data de contratação é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Contratação")]
        public DateTime HiringDate { get; set; }

        [Required(ErrorMessage = "O código PID do funcionário é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O PID do funcionário tem apenas 11 caracteres")]
        public string PID { get; set; }

        [Required(ErrorMessage = "O departamento é obrigatório.")]
        [ForeignKey("Department")]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        public virtual Department Department { get; set; }

        [NotMapped]
        public virtual IdentityRole Role { get; set; }

        //[NotMapped]
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }

    }
}