namespace MyTE.Models.ViewModel
{
    public class AdminRecordViewModel
    {
        public string UserEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalHours { get; set; }
        public List<RecordWithWBS> RecordsWithWBS { get; set; }
    }

    public class RecordWithWBS
    {
        public DateTime Data { get; set; }
        public double Hours { get; set; }
        public string WBSName { get; set; }
        public string WBSDescription { get; set; }
    }
}
