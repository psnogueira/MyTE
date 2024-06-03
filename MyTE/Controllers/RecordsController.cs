using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.DTO;
using MyTE.Models;
using MyTE.Models.ViewModel;
using MyTE.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTE.Controllers
{
    [Authorize(Policy = "Usuario")]
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly DateTime StartDateRestriction = new DateTime(2024, 1, 1);

        public RecordsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Records
        public async Task<IActionResult> Index(DateTime? dataSearch)
        {
            if (dataSearch < StartDateRestriction)
            {
                TempData["ErrorMessage"] = "A data de pesquisa deve ser maior que 01/01/2024";
            }
            var userId = _userManager.GetUserId(User);

            TempData["CurrentDate"] = dataSearch?.ToString("yyyy-MM-dd");

            var (year, month, dayInit, dayMax) = GetPeriodRange(dataSearch ?? DateTime.Now);

            var list = await GetRecordDTOList(userId, year, month, dayInit, dayMax);
            AddAdditionalRows(list, userId, year, month, dayInit, dayMax);

            ViewBag.WBSList = await GetWBSList();

            return View(list);
        }

        // POST: Records/Persist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Persist(List<RecordDTO> listRecordDTO)
        {
            if (ModelState.IsValid)
            {
                var records = listRecordDTO.SelectMany(dto => dto.records).ToList();

                if (ValidateRecords(ConvertForMap(records)))
                {
                    await SaveRecords(records);
                    TempData["SuccessMessage"] = "Registro de horas salvo com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Falha na validação dos registros.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para obter o intervalo de dias de um período
        private (int year, int month, int dayInit, int dayMax) GetPeriodRange(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            int dayInit = day <= 15 ? 1 : 16;
            int dayMax = day <= 15 ? 15 : DateTime.DaysInMonth(year, month);

            return (year, month, dayInit, dayMax);
        }

        // Método auxiliar para obter a lista de RecordDTOs para o usuário no período especificado
        private async Task<List<RecordDTO>> GetRecordDTOList(string userId, int year, int month, int dayInit, int dayMax)
        {
            var savedRecords = await _context.Record
                .Where(r => r.UserId == userId && r.Data.Year == year && r.Data.Month == month && r.Data.Day >= dayInit && r.Data.Day <= dayMax)
                .ToListAsync();

            var recordsGroupedByWBS = savedRecords.GroupBy(r => r.WBSId);
            List<RecordDTO> list = new List<RecordDTO>();
            double[] totalHoursDay = new double[16];

            foreach (var group in recordsGroupedByWBS)
            {
                var dto = await CreateRecordDTO(group, year, month, dayInit, dayMax, totalHoursDay);
                list.Add(dto);
            }

            return list;
        }

        // Método auxiliar para criar um RecordDTO a partir de um grupo de registros
        private async Task<RecordDTO> CreateRecordDTO(IGrouping<int, Record> group, int year, int month, int dayInit, int dayMax, double[] totalHoursDay)
        {
            int posicaoInicial = 0;
            double totalHoursWBS = 0d;
            List<Record> records = new List<Record>();

            for (int i = dayInit; i <= dayMax; i++)
            {
                var record = group.FirstOrDefault(r => r.Data.Day == i);
                if (record != null)
                {
                    record.SubmissionDate = DateTime.Now;
                    totalHoursWBS += record.Hours;
                    records.Add(record);
                    totalHoursDay[posicaoInicial] += record.Hours;
                }
                else
                {
                    record = new Record
                    {
                        Data = new DateTime(year, month, i),
                        UserId = _userManager.GetUserId(User),
                        WBSId = group.Key,
                        SubmissionDate = DateTime.Now
                };
                    
                    records.Add(record);
                }
                posicaoInicial++;
            }

            var wbs = await _context.WBS.FindAsync(group.Key);
            return new RecordDTO
            {
                WBS = wbs ?? new WBS { WBSId = 0, Code = "", Desc = "" },
                records = records,
                TotalHours = totalHoursWBS,
                TotalHoursDay = totalHoursDay
            };
        }

        // Método auxiliar para adicionar linhas adicionais se houver menos de 4 linhas
        private void AddAdditionalRows(List<RecordDTO> list, string userId, int year, int month, int dayInit, int dayMax)
        {
            while (list.Count < 4)
            {
                List<Record> records = new List<Record>();
                for (int i = dayInit; i <= dayMax; i++)
                {
                    records.Add(new Record
                    {
                        Data = new DateTime(year, month, i),
                        UserId = userId,
                        WBSId = 0,
                        SubmissionDate= DateTime.Now
                    });
                }
                list.Add(new RecordDTO
                {
                    WBS = new WBS { WBSId = 0, Code = "", Desc = "" },
                    records = records,
                    TotalHoursDay = new double[16]
                });
            }
        }

        // Método auxiliar para carregar a lista de WBS do banco de dados
        private async Task<List<SelectListItem>> GetWBSList()
        {
            var wbsList = await _context.WBS.ToListAsync();
            return wbsList.Select(wbs => new SelectListItem
            {
                Value = wbs.WBSId.ToString(),
                Text = $"{wbs.Code} - {wbs.Desc}"
            }).ToList();
        }

        // Método auxiliar para salvar os registros no banco de dados
        private async Task SaveRecords(List<Record> records)
        {
            var userId = records.First().UserId;
            var user = await _userManager.FindByIdAsync(userId);
            var userEmail = user.Email;
            var employeeName = user.FullName;
            var startDate = records.Min(r => r.Data);
            var endDate = records.Max(r => r.Data);
            var totalHours = records.Sum(r => r.Hours);

            // Filtra apenas os registros com horas > 0
            var recordsToSave = records.Where(r => r.Hours > 0).ToList();

            // Remove os registros antigos do banco de dados
            var recordsExclude = await _context.Record.Where(
                r => r.UserId == userId && r.Data >= startDate && r.Data <= endDate
            ).ToListAsync();

            _context.Record.RemoveRange(recordsExclude);

            // Salva apenas os registros com horas > 0
            _context.Record.AddRange(recordsToSave);

            // Atualiza ou cria um novo registro quinzenal
            var existingRecord = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .FirstOrDefaultAsync(b => b.UserEmail == userEmail && b.StartDate == startDate && b.EndDate == endDate && b.EmployeeName == employeeName);

            if (existingRecord != null)
            {
                existingRecord.TotalHours = totalHours;
                existingRecord.Records = recordsToSave;
                _context.Update(existingRecord);
            }
            else
            {
                var biweeklyRecord = new BiweeklyRecord
                {
                    UserEmail = userEmail,
                    EmployeeName = employeeName,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalHours = totalHours,
                    Records = recordsToSave
                };
                _context.BiweeklyRecords.Add(biweeklyRecord);
            }

            await _context.SaveChangesAsync();
        }

        // Método auxiliar para validar os registros
        private bool ValidateRecords(Dictionary<DateTime, double> myMap)
        {
            foreach (var item in myMap)
            {
                if (item.Value > 0 && item.Value < 8)
                {
                    TempData["ErrorMessageText"] = $"A data {item.Key:dd/MM} possui uma quantidade inferior ao mínimo permitido (8 horas). Quantidade de horas registradas: {item.Value}";
                    return false;
                }
                if (item.Value > 0 && (item.Key.DayOfWeek == DayOfWeek.Sunday || item.Key.DayOfWeek == DayOfWeek.Saturday))
                {
                    TempData["ErrorMessageText"] = $"A data {item.Key} não é considerada um dia útil.";
                    return false;
                }
                if (item.Value > 24)
                {
                    TempData["ErrorMessageText"] = $"A data {item.Key:dd/MM} possui uma quantidade superior ao máximo de horas de um dia (24 horas). Quantidade de horas registradas: {item.Value}";
                    return false;
                }
            }
            return true;
        }

        // Método auxiliar para converter a lista de registros em um dicionário
        private Dictionary<DateTime, double> ConvertForMap(List<Record> listRecords)
        {
            return listRecords
                .GroupBy(record => record.Data)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(record => record.Hours)
                );
        }

        [HttpPost]
        public IActionResult Navigate(string direction)
        {
            var currentDate = DateTime.Now;

            if (TempData["CurrentDate"] != null)
            {
                currentDate = DateTime.Parse(TempData["CurrentDate"].ToString());
            }

            // Ajusta a data atual baseada na direção de navegação
            currentDate = direction == "previous"
                ? currentDate.Day <= 15
                    ? new DateTime(currentDate.AddMonths(-1).Year, currentDate.AddMonths(-1).Month, 16)
                    : new DateTime(currentDate.Year, currentDate.Month, 1)
                : currentDate.Day >= 16
                    ? new DateTime(currentDate.AddMonths(1).Year, currentDate.AddMonths(1).Month, 1)
                    : new DateTime(currentDate.Year, currentDate.Month, 16);

            TempData["CurrentDate"] = currentDate.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { dataSearch = currentDate });
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> AdminView(string searchString, DateTime? startDate, DateTime? endDate, int? pageNumber)
        {
            var pageSize = 5;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            var biweeklyRecords = _context.BiweeklyRecords
                .Include(b => b.Records)
                .OrderByDescending(r => r.StartDate)
                .AsQueryable();

            // Filtros de busca
            if (!string.IsNullOrEmpty(searchString))
            {
                biweeklyRecords = biweeklyRecords.Where(r => r.UserEmail.Contains(searchString));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                biweeklyRecords = biweeklyRecords.Where(d => d.StartDate <= endDate.Value && d.EndDate >= startDate.Value);
            }
            else if (startDate.HasValue)
            {
                biweeklyRecords = biweeklyRecords.Where(d => d.StartDate >= startDate.Value);
            }
            else if (endDate.HasValue)
            {
                biweeklyRecords = biweeklyRecords.Where(d => d.EndDate <= endDate.Value);
            }

            // Criação do ViewModel para a View de Administração
            var viewModel = new AdminViewModel
            {
                ReportsList = await PaginatedList<BiweeklyRecord>.CreateAsync(biweeklyRecords.AsNoTracking(), pageNumber ?? 1, pageSize),
                CurrentFilter = searchString,
                BiweeklyRecord = new BiweeklyRecord(),
            };

            return View(viewModel);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> ViewDetails(int id)
        {
            var biweeklyRecord = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .FirstOrDefaultAsync(b => b.BiweeklyRecordId == id);

            if (biweeklyRecord == null)
            {
                return NotFound();
            }

            var recordDetails = biweeklyRecord.Records
                .OrderBy(r => r.Data)
                .Select(r => new RecordDetail
                {
                    Record = r,
                    WBS = _context.WBS.FirstOrDefault(w => w.WBSId == r.WBSId)
                }).ToList();

            var viewModel = new RecordDetailsViewModel
            {
                BiweeklyRecord = biweeklyRecord,
                RecordDetails = recordDetails
            };

            return View(viewModel);
        }
    }
}
