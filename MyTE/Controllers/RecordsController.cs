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

            if (dataSearch.HasValue)
            {
                TempData["CurrentDate"] = dataSearch.Value.ToString("yyyy-MM-dd");
            }
            else
            {
                TempData["CurrentDate"] = null;
            }

            var list = await GetRecordsListAsync(dataSearch, userId);

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
                var records = new List<Record>();
                foreach (var recordDTO in listRecordDTO)
                {
                    foreach (var record in recordDTO.records)
                    {
                        // Atribui a WBSId selecionada a cada registro
                        records.Add(record);
                    }
                }

                // Validação adicional para garantir que WBSId 0 só tenha horas iguais a 0
                foreach (var record in records)
                {
                    if (record.WBSId == 0 && record.Hours != 0)
                    {
                        TempData["ErrorMessage"] = "Falha na validação dos registros.";
                        TempData["ErrorMessageText"] = "Não é possível salvar horas na WBS vazia.";
                        return RedirectToAction(nameof(Index));
                    }
                }

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
                if (currentDate.Day <= 15)
                {
                    currentDate = currentDate.AddMonths(-1);
                    currentDate = new DateTime(currentDate.Year, currentDate.Month, 16);
                }
                else
                {
                    currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                }
            }
            else if (direction == "next")
            {
                if (currentDate.Day >= 16)
                {
                    currentDate = currentDate.AddMonths(1);
                    currentDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                }
                else
                {
                    currentDate = new DateTime(currentDate.Year, currentDate.Month, 16);
                }
            }

            TempData["CurrentDate"] = currentDate.ToString("yyyy-MM-dd");
            return RedirectToAction("Index", new { dataSearch = currentDate });
        }

        #region Private Methods

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

            // Carrega os registros salvos do banco de dados
            var savedRecords = await _context.Record
                .Where(r => r.UserId == userId && r.Data.Year == year && r.Data.Month == month && r.Data.Day >= dayInit && r.Data.Day <= dayMax)
                .ToListAsync();

            // Agrupa os registros por WBSId
            var recordsGroupedByWBS = savedRecords.GroupBy(r => r.WBSId);

            // Itera sobre os grupos de registros por WBSId
            foreach (var group in recordsGroupedByWBS)
            {
                int posicaoInicial = 0;
                double totalHoursWBS = 0d;
                List<Record> records = new List<Record>();
                for (int i = dayInit; i <= dayMax; i++)
                {
                    var record = group.FirstOrDefault(r => r.Data.Day == i);
                    if (record != null)
                    {
                        totalHoursWBS += record.Hours;
                        records.Add(record);
                        totalHoursDay[posicaoInicial] = totalHoursDay[posicaoInicial] + record.Hours;
                    }
                    else
                    {
                        record = new Record
                        {
                            Data = new DateTime(year, month, i),
                            UserId = userId,
                            WBSId = group.Key // Usa o WBSId do grupo
                        };
                        records.Add(record);
                    }
                    posicaoInicial++;
                }

                var wbs = await _context.WBS.FindAsync(group.Key);
                RecordDTO dto = new RecordDTO
                {
                    WBS = wbs ?? new WBS { WBSId = 0, Code = "", Desc = "" },
                    records = records,
                    TotalHours = totalHoursWBS,
                    TotalHoursDay = totalHoursDay
                };
                list.Add(dto);
            }

            // Adiciona linhas adicionais se houver menos de 4 linhas
            while (list.Count < 4)
            {
                int posicaoInicial = 0;
                double totalHoursWBS = 0d;
                List<Record> records = new List<Record>();
                for (int i = dayInit; i <= dayMax; i++)
                {
                    var record = new Record
                    {
                        Data = new DateTime(year, month, i),
                        UserId = userId,
                        WBSId = 0 // Inicialmente sem WBS associada
                    };
                    records.Add(record);
                    posicaoInicial++;
                }
                RecordDTO dto = new RecordDTO
                {
                    WBS = new WBS { WBSId = 0, Code = "", Desc = "" },
                    records = records,
                    TotalHours = totalHoursWBS,
                    TotalHoursDay = totalHoursDay
                };
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
            _context.Record.AddRange(recordsToSave);
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
                    TempData["ErrorMessageText"] = "A data " + item.Key.Date.ToString("dd/MM") + " possui uma quantidade superior ao máximo";
                    TempData["ErrorMessageText2"] = "de horas de um dia (24 horas). Quantidade de horas registradas: " + item.Value;
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

        #endregion
    }
}
