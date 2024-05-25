using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [Remote(action: "VerifyEmail", controller: "Users", ErrorMessage = "Este email já está em uso.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome do funcionário deve ter no máximo 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "O Nome não deve conter números ou caracteres especiais.")]
    [Display(Name = "Nome")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "O sobrenome do funcionário é obrigatório")]
    [MaxLength(50, ErrorMessage = "O sobrenome do funcionário deve ter até 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "O Sobrenome não deve conter números ou caracteres especiais.")]
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
    [Display(Name = "Departamento")]
    public int DepartmentId { get; set; }


    [Required(ErrorMessage = "O role é obrigatório.")]
    [Display(Name = "Nível de Acesso")]
    public string RoleId { get; set; }

    [Required(ErrorMessage = "A senha é obrigatório.")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#\$%\^&\*()\-\+=\[\]{};':""\\|,.<>\/?]).{6,}$",
    ErrorMessage = "A senha deve conter pelo menos 6 caracteres, incluindo um número, uma letra maiúscula, uma letra minúscula e um caractere especial.")]
    [Display(Name = "Senha")]
    public string Password { get; set; }


    [Required(ErrorMessage = "A confirmação de senha é obrigatório.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "As senhas digitadas não correspondem.")]
    [Display(Name = "Confirmar senha")]
    public string ConfirmPassword { get; set; }
}
