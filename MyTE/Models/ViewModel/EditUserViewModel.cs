using System;
using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string Id { get; set; }

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
}
