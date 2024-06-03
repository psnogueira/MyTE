using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTE.Models
{
    public class Record
    {
 
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordId { get; set; }

        [Required(ErrorMessage = "O ID do funcionário é obrigatório.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "O WBS é obrigatório.")]
        //[ForeignKey("WBS")]
        public int WBSId { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "A quantidade de horas é obrigatória.")]
        [Range(0, 24, ErrorMessage = "A quantidade de horas deve estar entre 0 e 24.")]
        public double Hours { get; set; }

        [DataType(DataType.Date)]
        public DateTime SubmissionDate { get; set; }

        public Record()
        { }

        public Record(string userId, int wbsId, DateTime data, double hours)
        {
            UserId = userId;
            WBSId = wbsId;
            Data = data;
            Hours = hours;
        }
    }

}
