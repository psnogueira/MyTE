using MyTE.Pagination;

namespace MyTE.Models.ViewModel
{
    public class DepartmentViewModel
    {
        public PaginatedList<Department> DepartmentList { get; set; }
        public Department Department { get; set; }
        public string CurrentFilter { get; set; }
    }
}
