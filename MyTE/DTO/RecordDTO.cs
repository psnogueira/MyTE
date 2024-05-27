using MyTE.Models;

namespace MyTE.DTO
{
    public class RecordDTO
    {
        public List<Record>? records { get; set; }
        public int WBSId { get; set; } // Adicione esta propriedade para armazenar o ID do WBS selecionado
        public WBS? WBS { get; set; }

        public double TotalHours { get; set; }

        public double[]? TotalHoursDay { get; set; }

    }
}
