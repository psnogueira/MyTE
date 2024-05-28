using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Data.Migrations;
using MyTE.DTO;
using MyTE.Models;
using MyTE.Models.ViewModel;
using MyTE.Pagination;
using System.Drawing.Printing;

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
            if(dataSearch < StartDateRestriction)
            {
                TempData["ErrorMessage"] = "A data de pesquisa deve ser maior que 01/01/2024";
            }
            var UserId = _userManager.GetUserId(User);

            var consultaWbs = from obj in _context.WBS select obj;
            var consultaRecord = from obj in _context.Record select obj;
            int year = 0;
            int month = 0;
            int day = 0;
            if (dataSearch != null && dataSearch > StartDateRestriction)
            {
                year = dataSearch.Value.Year;
                month = dataSearch.Value.Month;
                day = dataSearch.Value.Day;
            }
            else
            {
                year = DateTime.Now.Year;
                month = DateTime.Now.Month;
                day = DateTime.Now.Day;

            }
            int dayInit = 0;
            int dayMax = 0;

            if (day <= 15)
            {
                dayInit = 1;
                dayMax = 15;
            }
            else
            {
                dayInit = 16;
                dayMax = DateTime.DaysInMonth(year, month);
            }

            List<RecordDTO> list = new List<RecordDTO>();
            double[] totalHoursDay = new double[16];

            foreach (var wbs in consultaWbs.ToList())
            {
                int posicaoInicial = 0;
                double totalHoursWBS = 0d;
                List<Record> records = new List<Record>();
                for (int i = dayInit; i <= dayMax; i++)
                {


                    var consultaRecordFinal = consultaRecord.Where(s => s.UserId == UserId && s.Data == new DateTime(year, month, i) && s.WBSId == wbs.WBSId);

                    Record? result = consultaRecordFinal.FirstOrDefault();

                    if (result != null && result.RecordId > 0)
                    {
                        totalHoursWBS += result.Hours;
                        records.Add(result);
                        totalHoursDay[posicaoInicial] = totalHoursDay[posicaoInicial] + result.Hours;
                    }
                    else
                    {
                        Record record = new Record();
                        record.Data = new DateTime(year, month, i);
                        record.UserId = UserId;
                        record.WBSId = wbs.WBSId;
                        records.Add(record);
                    }
                    posicaoInicial++;
                }
                RecordDTO dto = new RecordDTO();
                dto.WBS = wbs;
                dto.records = records;
                dto.TotalHours = totalHoursWBS;
                dto.TotalHoursDay = totalHoursDay;
                list.Add(dto);
            }
            return View(await Task.FromResult(list));
        }

        // POST: Records/Persist
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Persist(List<RecordDTO> listRecordDTO)
        {
            if (ModelState.IsValid)
            {
                List<Record> records = new List<Record>();
                foreach (var item in listRecordDTO)
                {
                    records.AddRange(item.records);
                }
                if (ValidateRecords(ConvertForMap(records)))
                {
                    var userId = records.First().UserId;
                    var user = await _userManager.FindByIdAsync(userId);
                    var userEmail = user.Email;
                    var employeeName = user.FullName;
                    var startDate = records.Min(r => r.Data);
                    var endDate = records.Max(r => r.Data);
                    var totalHours = records.Sum(r => r.Hours);

                    // Check if a biweekly record already exists for this user and period
                    var existingRecord = await _context.BiweeklyRecords
                        .Include(b => b.Records)
                        .FirstOrDefaultAsync(b => b.UserEmail == userEmail && b.StartDate == startDate && b.EndDate == endDate && b.EmployeeName == employeeName);

                    if (existingRecord != null)
                    {
                        // Update existing record
                        existingRecord.TotalHours = totalHours;
                        existingRecord.Records = records;

                        _context.Update(existingRecord);
                    }
                    else
                    {
                        // Create a new record
                        var biweeklyRecord = new BiweeklyRecord
                        {
                            UserEmail = userEmail,
                            EmployeeName = employeeName,
                            StartDate = startDate,
                            EndDate = endDate,
                            TotalHours = totalHours,
                            Records = records
                        };

                        _context.BiweeklyRecords.Add(biweeklyRecord);
                    }

                    var recordsExclude = await _context.Record.Where(
                        r => r.UserId == userId
                        && r.Data >= startDate && r.Data <= endDate
                    ).ToListAsync();

                    _context.Record.RemoveRange(recordsExclude);
                    var recordsToSave = new List<Record>();
                    foreach (var itemRecord in records)
                    {
                        if (itemRecord.Hours > 0)
                        {
                            recordsToSave.Add(itemRecord);
                        }

                    }
                    TempData["SuccessMessage"] = "Registro de horas salvo com sucesso!";
                    _context.AddRange(recordsToSave);
                    _context.Record.AddRange(records);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Falha na validação dos registros.";
                    return RedirectToAction(nameof(Index));
                }
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ValidateRecords(Dictionary<DateTime, double> myMap)
        {
            foreach (var item in myMap)
            {
                if (item.Value > 0 && item.Value < 8)
                {
                    TempData["ErrorMessageText"] = "A data " + item.Key.Date.ToString("dd/MM") + " possui uma quantidade inferior ao minimo";
                    TempData["ErrorMessageText2"] = "permitido (8 horas). Quantidade de horas registradas: " + item.Value;
                    return false;
                }
                if (item.Value > 0 && (item.Key.DayOfWeek.Equals(DayOfWeek.Sunday) || item.Key.DayOfWeek.Equals(DayOfWeek.Saturday)))
                {
                    TempData["ErrorMessageText"] = "A data " + item.Key + " não é considerada um dia útil.";
                    return false;
                }
                if (item.Value > 24)
                {
                    TempData["ErrorMessageText"] = "A data " + item.Key + " possui uma quantidade superior ao máximo de horas de um dia (24 horas).";
                    TempData["ErrorMessageText2"] = "Quantidade de horas registradas: " + item.Value;
                    return false;
                }
            }
            return true;
        }

        private Dictionary<DateTime, double> ConvertForMap(List<Record> listRecords)
        {
            Dictionary<DateTime, double> myMap = new Dictionary<DateTime, double>();

            foreach (Record record in listRecords)
            {
                if (!myMap.ContainsKey(record.Data))
                {
                    myMap.Add(record.Data, record.Hours);
                }
                else
                {
                    double oldValue;
                    myMap.TryGetValue(record.Data, out oldValue);
                    myMap.Remove(record.Data);
                    myMap.Add(record.Data, record.Hours + oldValue);
                }
            }
            return myMap;
        }


        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> AdminView(string searchString, int? pageNumber)
        {
            var pageSize = 5;
            ViewData["CurrentFilter"] = searchString;

            var biweeklyRecords = _context.BiweeklyRecords
                                    .Include(b => b.Records)                                    
                                    .OrderByDescending(r => r.StartDate)
                                    .AsQueryable();

            if(!string.IsNullOrEmpty(searchString))
            {
                biweeklyRecords = biweeklyRecords
                    .Where(r => r.UserEmail.Contains(searchString));
            }

            var viewModel = new AdminViewModel
            {
                ReportsList = await PaginatedList<BiweeklyRecord>
                .CreateAsync(biweeklyRecords.AsNoTracking(), pageNumber ?? 1, pageSize),
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
                                              .OrderBy(r => r.Data)  // Ordenar registros por data
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

