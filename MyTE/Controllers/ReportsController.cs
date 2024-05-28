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

        public async Task<IActionResult> Index()
        {
            // Consulta LINQ para calcular a soma das horas da tabela Record para cada WBS.
            var horasPorWBS = await _context.Record
                                      .GroupBy(r => r.WBSId)
                                      .Select(g => new
                                      {
                                          WBSId = g.Key,
                                          TotalHoras = g.Sum(r => r.Hours)
                                      })
                                      .ToListAsync();

            // Ordena a lista de horas por WBS em ordem decrescente.
            var horasPorWBSList = horasPorWBS.OrderByDescending(h => h.TotalHoras).ToList();

            // Dicionário para armazenar os resultados.
            var horasPorWBSDicionario = horasPorWBSList.ToDictionary(item => item.WBSId, item => item.TotalHoras);

            // Consulta LINQ para obter as descrições das WBS.
            var wbsDescriptions = await _context.WBS
                                      .Select(w => new
                                      {
                                          w.WBSId,
                                          w.Desc
                                      })
                                      .ToListAsync();

            // Dicionário para armazenar as descrições das WBS.
            var wbsDescriptionsDictionary = wbsDescriptions.ToDictionary(item => item.WBSId, item => item.Desc);

            // ViewBag para exibir os dados na View.
            ViewBag.HorasPorWBS = horasPorWBSDicionario;
            ViewBag.DescricoesWBS = wbsDescriptionsDictionary;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportWBS()
        {
            // Consulta LINQ para obter a soma das horas da tabela Record para cada WBS.
            var horasPorWBS = await _context.Record
                                      .GroupBy(r => r.WBSId)
                                      .Select(g => new
                                      {
                                          WBSId = g.Key,
                                          TotalHoras = g.Sum(r => r.Hours)
                                      })
                                      .ToListAsync();

            // Ordena a lista de horas por WBS em ordem decrescente.
            var horasPorWBSList = horasPorWBS.OrderByDescending(h => h.TotalHoras).ToList();

            // Consulta LINQ para obter as descrições das WBS.
            var wbsDescriptions = await _context.WBS
                                      .Select(w => new
                                      {
                                          w.WBSId,
                                          w.Code,
                                          w.Desc
                                      })
                                      .ToListAsync();

            // Lista combinada com os Ids, Códigos, Descrições e Total de horas.
            var listaCombinada = (from h in horasPorWBSList
                                  join d in wbsDescriptions
                                  on h.WBSId equals d.WBSId
                                  select new
                                  {
                                      WBSId = h.WBSId,
                                      Code = d.Code,
                                      Desc = d.Desc,
                                      TotalHours = h.TotalHoras
                                  }).ToList();

            // Configuração do arquivo CSV para download.
            var fileName = $"Relatorio_{DateTime.Today.ToString("dd/MM/yyyy")}.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Id da WBS",
                "Código",
                "Descrição",
                "Total de Horas"
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(listaCombinada, columnNames);

            return File(csvData, contentType, fileName);
        }

    }
}
