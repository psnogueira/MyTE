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
}
