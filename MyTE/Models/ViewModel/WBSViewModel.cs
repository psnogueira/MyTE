using Microsoft.AspNetCore.Mvc.Rendering;
using MyTE.Pagination;

namespace MyTE.Models.ViewModel
{
    public class WBSViewModel
    {
        public PaginatedList<WBS> WBSList { get; set; }
        public SelectList Type { get; set; }
        public WBS WBS { get; set; }
        public string CurrentFilter { get; set; }
    }
}
