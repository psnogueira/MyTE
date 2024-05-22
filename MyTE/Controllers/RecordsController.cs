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

        public RecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        // GET: Records
        public async Task<IActionResult> Index()
        {

            var consultaWbs = from obj in _context.WBS select obj;
            var consultaRecord = from obj in _context.Record select obj;

            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
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
                    var consultaRecordFinal = consultaRecord.Where(s => s.EmployeeId == 1 && s.Data == new DateTime(year, month, i) && s.WBSId == wbs.WBSId);

                    Record? result = consultaRecordFinal.FirstOrDefault();

                    if (result != null && result.RecordId > 0) {
                        totalHoursWBS += result.Hours;
                        records.Add(result);
                        totalHoursDay[posicaoInicial] = totalHoursDay[posicaoInicial] + result.Hours;
                    } else {
                        Record record = new Record();
                        record.Data = new DateTime(year, month, i);
                        //pegar usuario da sessao, entender como fazer isso
                        record.EmployeeId = 1;
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

        // GET: Records/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Record
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (@record == null)
            {
                return NotFound();
            }

            return View(@record);
        }

        // GET: Records/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Records/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecordId,EmployeeId,WBSId,Data,Hours")] Record record)
        {
            if (ModelState.IsValid)
            {
                _context.Add(record);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                
            }
            return View(record);
        }


        // GET: Records/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Record.FindAsync(id);
            if (@record == null)
            {
                return NotFound();
            }
            return View(@record);
        }

        // POST: Records/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RecordId,EmployeeId,WBSId,Date,Hours")] Record @record)
        {
            if (id != @record.RecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@record);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecordExists(@record.RecordId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@record);
        }

        // GET: Records/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @record = await _context.Record
                .FirstOrDefaultAsync(m => m.RecordId == id);
            if (@record == null)
            {
                return NotFound();
            }

            return View(@record);
        }

        // POST: Records/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @record = await _context.Record.FindAsync(id);
            if (@record != null)
            {
                _context.Record.Remove(@record);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecordExists(int id)
        {
            return _context.Record.Any(e => e.RecordId == id);
        }

        //#########################################################################

        private bool ValidateRecords(Dictionary<DateTime, double> myMap)
        {
            foreach (var item in myMap)
            {
                if (item.Value < 8)
                {
                    Console.WriteLine("A Data: " + item.Key + " possui uma quantidade inferior ao minimo permitido (8 horas). Quantidade de horas registradas: " + item.Value);
                    //throw new Exception("A Data: " + item.Key + " possui uma quantidade inferior ao minimo permitido (8 horas). Quantidade de horas registradas: " + item.Value);
                    return false;
                }
                if (item.Key.DayOfWeek.Equals(DayOfWeek.Sunday) || item.Key.DayOfWeek.Equals(DayOfWeek.Saturday))
                {
                    Console.WriteLine("A Data: " + item.Key + " não é considerada um dia útil.");
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


        private static void Preenchelist(List<Record> listRecord)
        {
            listRecord.AddRange(new[] {
                new Record(1,1,new DateTime(2024,05,17), 6),
                new Record(1,2, new DateTime(2024,05,15), 8),
                new Record(1,1, new DateTime(2024,05,16), 8),
                new Record(1,2, new DateTime(2024,05,17), 4)
            });
        }
    }

}
