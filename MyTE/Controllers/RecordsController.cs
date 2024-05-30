using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.DTO;
using MyTE.Models;

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
            dataSearch = InitializeDataSearch(dataSearch);

            var list = await GetRecordsListAsync(dataSearch.Value, userId);

            var wbsList = await _context.WBS.ToListAsync();

            ViewBag.WBSList = GetWBSListItems(wbsList);

            return View(list);
        }

        // POST: Records/Persist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Persist(List<RecordDTO> listRecordDTO)
        {
            if (ModelState.IsValid)
            {
                var records = listRecordDTO.SelectMany(item => item.records).ToList();

                if (ValidateRecords(ConvertForMap(records)))
                {
                    await SaveRecordsAsync(records);
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

        [HttpPost]
        public IActionResult Navigate(string direction)
        {
            var currentDate = GetOrSetCurrentDate();

            if (direction == "previous")
            {
                currentDate = currentDate.AddDays(-15);
            }
            else if (direction == "next")
            {
                currentDate = currentDate.AddDays(15);
            }

            TempData["CurrentDate"] = currentDate.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { dataSearch = currentDate });
        }

        #region Private Methods

        private DateTime InitializeDataSearch(DateTime? dataSearch)
        {
            if (!dataSearch.HasValue)
            {
                dataSearch = TempData["CurrentDate"] != null ? DateTime.Parse(TempData["CurrentDate"].ToString()) : DateTime.Now;
            }

            return dataSearch.Value;
        }

        private async Task<List<RecordDTO>> GetRecordsListAsync(DateTime? dataSearch, string userId)
        {
            var list = new List<RecordDTO>();
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

            double[] totalHoursDay = new double[16];

            foreach (var wbs in consultaWbs.ToList())
            {
                int posicaoInicial = 0;
                double totalHoursWBS = 0d;
                List<Record> records = new List<Record>();
                for (int i = dayInit; i <= dayMax; i++)
                {
                    var consultaRecordFinal = consultaRecord.Where(s => s.UserId == userId && s.Data == new DateTime(year, month, i) && s.WBSId == wbs.WBSId);

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
                        record.UserId = userId;
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

            return list;
        }

        public List<SelectListItem> GetWBSListItems(List<WBS> wbsList)
        {
            return wbsList.Select(wbs => new SelectListItem
            {
                Value = wbs.WBSId.ToString(),
                Text = $"{wbs.Code} - {wbs.Desc}"
            }).ToList();
        }

        private async Task SaveRecordsAsync(List<Record> records)
        {
            var userId = records.FirstOrDefault()?.UserId;
            var startDate = records.Min(r => r.Data);
            var endDate = records.Max(r => r.Data);

            var existingRecords = await _context.Record.Where(r => r.UserId == userId && r.Data >= startDate && r.Data <= endDate).ToListAsync();

            var recordsToSave = new List<Record>();

            var recordsToDelete = new List<Record>();

            foreach (var record in records) 
            {
                var existingRecord = existingRecords.FirstOrDefault(r => r.Data == record.Data && r.WBSId == record.WBSId);

                if (existingRecord != null)
                {
                    if (record.Hours > 0)
                    {
                        existingRecord.Hours = record.Hours;
                        recordsToSave.Add(existingRecord);
                    }
                    else
                    {
                        recordsToDelete.Add(existingRecord);
                    }
                }
                else if (record.Hours > 0)
                {
                    recordsToSave.Add(record);
                }
            }

            if (recordsToSave.Any())
            {
                _context.Record.UpdateRange(recordsToSave);
            }

            if (recordsToDelete.Any())
            {
                _context.Record.RemoveRange(recordsToDelete);
            }

            await _context.SaveChangesAsync();
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
                    TempData["ErrorMessage"] = "A data " + item.Key + " possui uma quantidade superior ao máximo de horas de um dia (24 horas).";
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

        private DateTime GetOrSetCurrentDate()
        {
            var currentDate = DateTime.Now;

            if (TempData["CurrentDate"] != null)
            {
                currentDate = DateTime.Parse(TempData["CurrentDate"].ToString());
            }

            return currentDate;
        }

        #endregion
    }
}
