using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;

namespace MyTE.Models.Map
{
    public class DepartmentMap : ClassMap<Department>
    {
        public DepartmentMap()
        {
            Map(m => m.DepartmentId).Name("Id");
            Map(m => m.Name).Name("Nome");
        }
    }
}
