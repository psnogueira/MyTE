using System.ComponentModel.DataAnnotations.Schema;

namespace MyTE.Models
{
    public class BiweeklyRecord
    {
        public int BiweeklyRecordId { get; set; }
        public string UserEmail { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalHours { get; set; }
        public List<Record> Records { get; set; }
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}
