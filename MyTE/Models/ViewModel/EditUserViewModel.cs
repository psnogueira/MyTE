using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyTE.Pagination;
using System.ComponentModel.DataAnnotations;

namespace MyTE.Models.ViewModel
{
    public class EditUserViewModel
    {
        public PaginatedList<ApplicationUser> UserList { get; set; }

        public ApplicationUser User { get; set; }
        public string CurrentFilter { get; set; }

        public Dictionary<string, IList<string>> UserRoles { get; set; }
        public Department Department { get; set; }
        public List<SelectListItem> Departments { get; set; }

        [Display(Name = "Nível de Acesso")]
        public string Role { get; set; }

        public List<SelectListItem> RolesList { get; set; }


        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime HiringDate { get; set; }
        public string PID { get; set; }
    }
}
