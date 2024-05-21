using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyTE.Models.ViewModel
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime HiringDate { get; set; }
        public Department Department { get; set; }
        public string PID { get; set; }

        public List<SelectListItem> Departments { get; set; }
        public IdentityRole Role {  get; set; }
        public List<SelectListItem> Roles { get; set; }
    }
}
