using System;
using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Display(Name = "Nome")]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Sobrenome")]
    public string LastName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de Contratação")]
    public DateTime HiringDate { get; set; }

    [Required]
    public string PID { get; set; }

    [Required]
    [Display(Name = "Departamento")]
    public int DepartmentId { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string RoleId { get; set; }
}
