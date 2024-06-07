using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "O email é obrigatório.")]
    [Remote(action: "VerifyEmail", controller: "Users", AdditionalFields = nameof(Id), ErrorMessage = "Este email já está em uso.")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome do funcionário deve ter no máximo 50 caracteres.")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\s-]*$", ErrorMessage = "O Nome não deve conter números ou caracteres especiais.")]
    [Display(Name = "Nome")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "O sobrenome do funcionário é obrigatório")]
    [MaxLength(50, ErrorMessage = "O sobrenome do funcionário deve ter até 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\s-]*$", ErrorMessage = "O Sobrenome não deve conter números ou caracteres especiais.")]
    [Display(Name = "Sobrenome")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "A data de contratação é obrigatória.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Contratação")]
    public DateTime HiringDate { get; set; }

    [Required(ErrorMessage = "O código PID do funcionário é obrigatório.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O PID do funcionário tem apenas 11 caracteres")]
    public string? PID { get; set; }

    [Required(ErrorMessage = "O departamento é obrigatório.")]
    [Display(Name = "Departamento")]
    public int DepartmentId { get; set; }


    [Required(ErrorMessage = "O role é obrigatório.")]
    [Display(Name = "Nível de Acesso")]
    public string? RoleId { get; set; }

}
