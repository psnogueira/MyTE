using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.DTO;
using MyTE.Models;

namespace MyTE.Controllers
{
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecordsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Records
        public async Task<IActionResult> Index(DateTime? dataSearch)
        {

            var UserId = _userManager.GetUserId(User);

            var consultaWbs = from obj in _context.WBS select obj;
            var consultaRecord = from obj in _context.Record select obj;
            int year = 0;
            int month = 0;
            int day = 0;
            if (dataSearch != null)
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

            if (day <= 15) {
                dayInit = 1;
                dayMax = 15;
            } else {
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
                for (int i = dayInit; i <= dayMax; i++) {

                    //pegar usuario da sessao, entender como fazer isso
                    var consultaRecordFinal = consultaRecord.Where(s => s.UserId == UserId && s.Data == new DateTime(year, month, i) && s.WBSId == wbs.WBSId);

                    Record? result = consultaRecordFinal.FirstOrDefault();

                    if (result != null && result.RecordId > 0) {
                        totalHoursWBS += result.Hours;
                        records.Add(result);
                        totalHoursDay[posicaoInicial] = totalHoursDay[posicaoInicial] + result.Hours;
                    } else {
                        Record record = new Record();
                        record.Data = new DateTime(year, month, i);
                        //pegar usuario da sessao, entender como fazer isso
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
                            && r.Data >= records[0].Data && r.Data <= records[records.Count()-1].Data
                        ).ToListAsync();
                    _context.Record.RemoveRange(recordsExclude);
                    _context.AddRange(records);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
                
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ValidateRecords(Dictionary<DateTime, double> myMap)
        {
            foreach (var item in myMap)
            {
                if (item.Value > 0 && item.Value < 8)
                {
                    Console.WriteLine("A Data: " + item.Key + " possui uma quantidade inferior ao minimo permitido (8 horas). Quantidade de horas registradas: " + item.Value);
                    //throw new Exception("A Data: " + item.Key + " possui uma quantidade inferior ao minimo permitido (8 horas). Quantidade de horas registradas: " + item.Value);
                    return false;
                }
                if (item.Value > 0 && (item.Key.DayOfWeek.Equals(DayOfWeek.Sunday) || item.Key.DayOfWeek.Equals(DayOfWeek.Saturday)))
                {
                    Console.WriteLine("A Data: " + item.Key + " não é considerada um dia útil.");
                    return false;
                }
                if (item.Value > 24)
                {
                    Console.WriteLine("A Data: " + item.Key + " possui uma quantidade superior ao máximo de horas de um dia (24 horas). Quantidade de horas registradas: " + item.Value);
                    //throw new Exception("A Data: " + item.Key + " possui uma quantidade superior ao máximo de horas de um dia (24 horas). Quantidade de horas registradas: " + item.Value);
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

    }
}
