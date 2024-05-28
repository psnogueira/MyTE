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

        public async Task<IActionResult> Index(int departmentId = 2)
        {
            // Configura a consulta para incluir o departamento do usuário.
            var usersQuery = _context.Users.Include(u => u.Department).AsQueryable();

            // Filtrar Users pelo ID do departamento.
            if (departmentId != 0)
            {
                usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId);
            }

            // Transforma a consulta em uma lista para ser exibida na View.
            var usersList = await usersQuery.ToListAsync();

            // Consulta para obter as WBS.
            //var wbsQuery = _context.WBS.AsQueryable();

            return View(usersList);
        }

        [HttpPost]
        public async Task<IActionResult> ExportService()
        {
            // Lista de Departamentos do banco de dados.
            var records = await _context.Department.ToListAsync();

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
