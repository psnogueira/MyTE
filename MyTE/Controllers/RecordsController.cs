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
            if(dataSearch < StartDateRestriction)
            {
                TempData["ErrorMessage"] = "A data de pesquisa deve ser maior que 01/01/2024";
            }
            var UserId = _userManager.GetUserId(User);

            
            if (dataSearch.HasValue)
            {
                TempData["CurrentDate"] = dataSearch.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                TempData["CurrentDate"] = null;
            }
            

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

                    var recordsExclude = await _context.Record.Where(
                            r => r.UserId == records[0].UserId
                            && r.Data >= records[0].Data && r.Data <= records[records.Count() - 1].Data
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

        [HttpPost]
        public IActionResult Navigate(string direction)
        {
            var currentDate = DateTime.Now;

            if (TempData["CurrentDate"] == null)
            {
                TempData["CurrentDate"] = currentDate.ToString("yyyy-MM-dd");

            }
            else
            {
                currentDate = DateTime.Parse(TempData["CurrentDate"].ToString());
            }

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
    }
}
