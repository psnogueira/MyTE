using MyTE.Pagination;
using System.ComponentModel.DataAnnotations;

namespace MyTE.Models.ViewModel
{
    public class UserViewModel
    {
        public PaginatedList<ApplicationUser> DepartmentList { get; set; }
        public ApplicationUser Users { get; set; }
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email {  get; set; }
        public string PID { get; set; }
        public string Department { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime HireDate { get; set; }
    }
}
