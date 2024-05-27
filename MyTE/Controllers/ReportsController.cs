using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using CsvHelper;
using CsvHelper.Configuration;
using System.Text;
using MyTE.Models.Map;
using MyTE.Models;
using MyTE.Data;
using MyTE.Services;

namespace MyTE.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CSVService _csvService;
        //CSVService csvService

        public ReportsController(ApplicationDbContext context, CSVService csvService)
        {
            _context = context;
            _csvService = csvService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ExportService()
        {
            // Lista de Departamentos do banco de dados.
            var records = _context.Department.ToList();

            // Configuração do arquivo CSV para download.
            var fileName = "Reports.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Id do Departamento",
                "Nome do Departamento"
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(records, columnNames);

            return File(csvData, contentType, fileName);
        }
    }
}
