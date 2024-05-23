using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreateUserViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public DateTime HiringDate { get; set; }

    [Required]
    public string PID { get; set; }

    public int DepartmentId { get; set; }

    [Required]
    public string RoleId { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
