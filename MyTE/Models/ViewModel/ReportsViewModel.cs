using Microsoft.AspNetCore.Mvc.Rendering;
using MyTE.Pagination;

namespace MyTE.Models.ViewModel
{
    public class RecordDetailsViewModel
    {
        public BiweeklyRecord BiweeklyRecord { get; set; }
        public List<RecordDetail> RecordDetails { get; set; }

    }
    public class RecordDetail
    {
        public Record Record { get; set; }
        public WBS WBS { get; set; }
    }

    public class AdminViewModel
    {
        public PaginatedList<BiweeklyRecord> ReportsList { get; set; }
        public BiweeklyRecord BiweeklyRecord { get; set; }
        public ApplicationUser User { get; set; }
        public string CurrentFilter { get; set; }

        public double TotalHours { get; set; }
        public DateTime? CurrentStartDate { get; set; }
        public DateTime? CurrentEndDate { get; set;}
        public int? DepartmentType { get; set; } // Adicionar a propriedade DepartmentType
        public List<SelectListItem> DepartmentList { get; set; } // Adicionar a propriedade DepartmentList
    }
}
