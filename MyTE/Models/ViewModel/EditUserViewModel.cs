using Microsoft.AspNetCore.Mvc.Rendering;
using MyTE.Models;
using MyTE.Pagination;
using System;
using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
    [Display(Name = "Nome")]
    [StringLength(50, ErrorMessage = "O nome do funcionário deve ter até 50 caracteres.")]
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
    [StringLength(11, MinimumLength = 11, ErrorMessage = "O código PID deve ter 11 caracteres")]
    public string PID { get; set; }

    [Required(ErrorMessage = "O departamento é obrigatório.")]
    [Display(Name = "Departamento")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "A role é obrigatória.")]
    [Display(Name = "Role")]
    public string RoleId { get; set; }

}
