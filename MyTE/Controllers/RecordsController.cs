using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.DTO;
using MyTE.Models;
using MyTE.Models.Enum;
using MyTE.Models.ViewModel;
using MyTE.Pagination;
using MyTE.Services;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Globalization;
using System.Text;

namespace MyTE.Controllers
{
    [Authorize(Policy = "Usuario")]
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly DateTime StartDateRestriction = new DateTime(2024, 1, 1);

        private readonly CSVService _csvService;

        public RecordsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, CSVService csvService)
        {
            _context = context;
            _userManager = userManager;
            _csvService = csvService;
        }

        // GET: Records
        public async Task<IActionResult> Index(DateTime? dataSearch)
        {
            if (dataSearch < StartDateRestriction)
            {
                TempData["ErrorMessage"] = "A data de pesquisa deve ser maior que 01/01/2024";
                return RedirectToAction(nameof(Index));
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

            if (savedRecords.Any())
            {
                var latestSubmissionDate = savedRecords.Max(r => r.SubmissionDate);
                TempData["LastSubmissionDate"] = $"Salvo pela última vez em {latestSubmissionDate:dd/MM/yyyy HH:mm:ss}";
            }
            else
            {
                TempData["LastSubmissionDate"] = "";
            }


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

            // Muda lógica do totalHoursDay
            int totalDaysInPeriod = dayMax - dayInit + 1;
            totalHoursDay = new double[totalDaysInPeriod];

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
            int totalDaysInPeriod = dayMax - dayInit + 1;
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
                    TotalHoursDay = new double[totalDaysInPeriod]
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
            var departmentId = user.DepartmentId;
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
                .FirstOrDefaultAsync(b => b.UserEmail == userEmail 
                && b.StartDate == startDate 
                && b.EndDate == endDate 
                && b.EmployeeName == employeeName 
                && b.DepartmentId == departmentId);

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
                    DepartmentId = departmentId,
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
                    TempData["ErrorMessageText"] = $"A data {item.Key:dd/MM} possui uma quantidade inferior ao mínimo";
                    TempData["ErrorMessageText2"] = $"permitido (8 horas). Quantidade de horas registradas: {item.Value}";
                    return false;
                }
                if (item.Value > 0 && (item.Key.DayOfWeek == DayOfWeek.Sunday || item.Key.DayOfWeek == DayOfWeek.Saturday))
                {
                    TempData["ErrorMessageText"] = $"A data {item.Key} não é considerada um dia útil.";
                    return false;
                }
                if (item.Value > 24)
                {
                    TempData["ErrorMessageText"] = $"A data {item.Key:dd/MM} possui uma quantidade superior ao máximo de";
                    TempData["ErrorMessageText2"] = $"horas de um dia (24 horas). Quantidade de horas registradas: {item.Value}";
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
        public async Task<IActionResult> AdminView(string searchString, DateTime? startDate, DateTime? endDate,int? pageNumber, int? departmentType)
        {
            var pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["DepartmentType"] = departmentType; // Adicionar o departamento selecionado

            var biweeklyRecords = _context.BiweeklyRecords
                                    .Include(b => b.Records)    
                                    .Include(b => b.Department)
                                    .OrderByDescending(r => r.StartDate)
                                    .AsQueryable();

            var departments = await _context.Department.ToListAsync();
            var departmentList = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.Name
            }).ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                biweeklyRecords = biweeklyRecords
                    .Where(r => r.UserEmail.Contains(searchString) ||
                                r.EmployeeName.Contains(searchString));
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

            if (departmentType.HasValue && departmentType != 0)
            {
                biweeklyRecords = biweeklyRecords.Where(d => d.DepartmentId == departmentType);
            }

            var totalHours = await biweeklyRecords.SumAsync(b => b.TotalHours);

            var viewModel = new AdminViewModel
            {
                ReportsList = await PaginatedList<BiweeklyRecord>.CreateAsync(biweeklyRecords.AsNoTracking(), pageNumber ?? 1, pageSize),
                CurrentFilter = searchString,
                TotalHours = totalHours,
                BiweeklyRecord = new BiweeklyRecord(),
                DepartmentType = departmentType, // Adicionar departamento selecionado ao ViewModel
                DepartmentList = departmentList // Adicionar a lista de departamentos ao ViewModel
            };

            return View(viewModel);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> ViewDetails(int id)
        {
            var biweeklyRecord = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .FirstOrDefaultAsync(b => b.BiweeklyRecordId == id);

            ViewBag.RecordId = id;

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

        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> AdminViewWBS(string searchString, int? pageNumber, WBSType? wbsType)
        {
            int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            var wbs = from s in _context.WBS
                      select s;

            IQueryable<WBSType> wbsQuery = from m in _context.WBS
                                           orderby m.Type
                                           select m.Type;

            // Filtros de busca
            if (!string.IsNullOrEmpty(searchString))
            {
                wbs = wbs.Where(s => s.Code.Contains(searchString)
                                       || s.Desc.Contains(searchString));
            }

            if (wbsType.HasValue)
            {
                wbs = wbs.Where(x => x.Type == wbsType.Value);
            }
            var viewModel = new WBSViewModel
            {
                WBSList = await PaginatedList<WBS>.CreateAsync(wbs.AsNoTracking(), pageNumber ?? 1, pageSize),
                Type = new SelectList(await wbsQuery.Distinct().ToListAsync()),
                WBS = new WBS(),
                CurrentFilter = searchString
            };

            // Consulta LINQ para calcular a soma das horas da tabela Record para cada WBS.
            var horasPorWBS = await _context.Record
                                      .GroupBy(r => r.WBSId)
                                      .Select(g => new
                                      {
                                          WBSId = g.Key,
                                          TotalHoras = g.Sum(r => r.Hours)
                                      })
                                      .ToListAsync();

            // Juntar as WBS presentes na tabela Record com as WBS restantes da tabela WBS.
            var wbsIds = await _context.WBS.Select(w => w.WBSId).ToListAsync();
            var wbsIdsFromRecords = horasPorWBS.Select(h => h.WBSId).ToList();
            var wbsIdsToInclude = wbsIds.Except(wbsIdsFromRecords).ToList();
            var wbsToInclude = wbsIdsToInclude.Select(w => new
            {
                WBSId = w,
                TotalHoras = 0.0
            }).ToList();

            // Juntar horasPorWBS com wbsToInclude.
            horasPorWBS.AddRange(wbsToInclude);

            var horasPorWBSList = horasPorWBS.OrderByDescending(h => h.TotalHoras).ToList();

            // Cria uma nova lista de horasPorWBS com base no pageNumber e no pageSize.
            var listaPorPagina = horasPorWBSList.Skip(((pageNumber - 1) ?? 1 - 1) * pageSize).Take(pageSize).ToList();

            // Dicionário para armazenar os resultados.
            var horasPorWBSDicionario = listaPorPagina.ToDictionary(item => item.WBSId, item => item.TotalHoras);

            // Consulta LINQ para obter as descrições das WBS.
            var wbsDescriptions = await _context.WBS
                                      .Select(w => new
                                      {
                                          w.WBSId,
                                          w.Code,
                                          w.Type,
                                          w.Desc
                                      })
                                      .ToListAsync();

            // Dicionário para armazenar as descrições das WBS.
            var wbsDescriptionsDictionary = wbsDescriptions.ToDictionary(item => item.WBSId, item => item.Desc);
            var wbsCodeDictionary = wbsDescriptions.ToDictionary(item => item.WBSId, item => item.Code);
            var wbsTypeDictionary = wbsDescriptions.ToDictionary(item => item.WBSId, item => Enum.GetName(typeof(WBSType), item.Type));

            // ViewBag para exibir os dados na View.
            ViewBag.HorasPorWBS = horasPorWBSDicionario;
            ViewBag.DescricoesWBS = wbsDescriptionsDictionary;
            ViewBag.CodigosWBS = wbsCodeDictionary;
            ViewBag.TiposWBS = wbsTypeDictionary;

            return View(viewModel);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        public async Task<IActionResult> ViewDetailsWBS(int id)
        {
            // WBS selecionada.
            var wbs = await _context.WBS.FindAsync(id);

            // Lista de todas as quinzenas que possuem registros para a WBS selecionada.
            var biweeklyRecords = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .Where(b => b.Records.Any(r => r.WBSId == id))
                .ToListAsync();

            var isEmpty = true;

            if (wbs != null && biweeklyRecords.Count > 0)
            {
                // Lista de todas as quinzenas diferentes em biweeklyRecords.
                var biweeklyRecordsDistinct = biweeklyRecords.Select(b => new
                {
                    b.StartDate,
                    b.EndDate
                }).Distinct().ToList();

                // Ordena a lista de quinzenas em ordem decrescente.
                biweeklyRecordsDistinct = biweeklyRecordsDistinct.OrderByDescending(b => b.StartDate).ToList();

                // Array contendo a data mais antiga e a data mais recente.
                var dateRange = new DateTime[] { biweeklyRecordsDistinct.Min(b => b.StartDate), biweeklyRecordsDistinct.Max(b => b.EndDate) };

                // Lista de todos os registros de horas em biweeklyRecords.
                var records = biweeklyRecords.SelectMany(b => b.Records).Where(r => r.WBSId == id).ToList();

                // Dicionário para armazenar as quinzenas e a soma das horas de cada uma.
                var horasPorQuinzena = biweeklyRecordsDistinct.ToDictionary(b => b, b => records.Where(r => r.Data >= b.StartDate && r.Data <= b.EndDate).Sum(r => r.Hours));


                // Soma de todas as horas da WBS.
                var totalHoras = records.Sum(r => r.Hours);

                // Variável para indicar que a WBS não está vazia.
                isEmpty = false;

                // ViewBag para exibir os dados na View.
                ViewBag.WBS = wbs;
                ViewBag.Periodo = dateRange;
                ViewBag.HorasPorQuinzena = horasPorQuinzena;
                ViewBag.TotalHoras = totalHoras;
            }
            else
            {
                // ViewBag para exibir os dados na View.
                ViewBag.WBS = wbs;
                ViewBag.Periodo = null;
                ViewBag.HorasPorQuinzena = null;
                ViewBag.TotalHoras = null;
            }

            ViewBag.IsEmpty = isEmpty;

            return View();
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
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

            // Juntar as WBS presentes na tabela Record com as WBS restantes da tabela WBS.
            var wbsIds = await _context.WBS.Select(w => w.WBSId).ToListAsync();
            var wbsIdsFromRecords = horasPorWBS.Select(h => h.WBSId).ToList();
            var wbsIdsToInclude = wbsIds.Except(wbsIdsFromRecords).ToList();
            var wbsToInclude = wbsIdsToInclude.Select(w => new
            {
                WBSId = w,
                TotalHoras = 0.0
            }).ToList();

            // Juntar horasPorWBS com wbsToInclude.
            horasPorWBS.AddRange(wbsToInclude);

            // Ordena a lista de horas por WBS em ordem decrescente.
            var horasPorWBSList = horasPorWBS.OrderByDescending(h => h.TotalHoras).ToList();

            // Consulta LINQ para obter as descrições das WBS.
            var wbsDescriptions = await _context.WBS
                                      .Select(w => new
                                      {
                                          w.WBSId,
                                          w.Code,
                                          w.Desc,
                                          w.Type
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
                                      Type = Enum.GetName(typeof(WBSType), d.Type),
                                      TotalHours = h.TotalHoras
                                  }).ToList();

            // Configuração do arquivo CSV para download.
            var fileName = $"Relatorio_WBS_{DateTime.Today.ToString("dd/MM/yyyy")}.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Id da WBS",
                "Código",
                "Descrição",
                "Tipo",
                "Total de Horas"
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(listaCombinada, columnNames);

            // Retornar o arquivo CSV para download.
            return File(csvData, contentType, fileName);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportWBSDetails(int id)
        {
            // WBS selecionada.
            var wbs = await _context.WBS.FindAsync(id);

            // Lista de todas as quinzenas que possuem registros para a WBS selecionada.
            var biweeklyRecords = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .Where(b => b.Records.Any(r => r.WBSId == id))
                .ToListAsync();

            if (wbs == null || biweeklyRecords.Count <= 0)
            {
                return NotFound();
            }

            // Lista de todas as quinzenas diferentes em biweeklyRecords.
            var biweeklyRecordsDistinct = biweeklyRecords.Select(b => new
            {
                b.StartDate,
                b.EndDate
            }).Distinct().ToList();

            // Ordena a lista de quinzenas em ordem decrescente.
            biweeklyRecordsDistinct = biweeklyRecordsDistinct.OrderByDescending(b => b.StartDate).ToList();

            // Lista de todos os registros de horas em biweeklyRecords.
            var records = biweeklyRecords.SelectMany(b => b.Records).Where(r => r.WBSId == id).ToList();

            // Lista para armazenar as quinzenas e a soma das horas de cada uma.
            var listaCombinada = biweeklyRecordsDistinct.Select(b => new
            {
                Quinzena = $"{b.StartDate:dd/MM/yyyy} - {b.EndDate:dd/MM/yyyy}",
                TotalHoras = records.Where(r => r.Data >= b.StartDate && r.Data <= b.EndDate).Sum(r => r.Hours)
            }).ToList();

            // Configuração do arquivo CSV para download.
            var fileName = $"Relatorio_{wbs.Desc}_{DateTime.Today.ToString("dd/MM/yyyy")}.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Quinzena",
                "Total de Horas"
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(listaCombinada, columnNames);

            // Retornar o arquivo CSV para download.
            return File(csvData, contentType, fileName);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportEmployees()
        {
            // Consulta LINQ para obter os registros de horas dos funcionários.
            var employeesRecords = await _context.BiweeklyRecords
                .Include(b => b.Department)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();

            // Lista contendo os dados de employeesRecords mais o PID do funcionário.
            var listaCombinada = employeesRecords.Select(b => new
            {
                Id = b.BiweeklyRecordId,
                PID = _context.Users.FirstOrDefault(u => u.Email == b.UserEmail).PID,
                EmployeeName = b.EmployeeName,
                Email = b.UserEmail,
                Department = b.Department.Name,
                StartDate = b.StartDate.ToString("dd/MM/yyyy"),
                EndDate = b.EndDate.ToString("dd/MM/yyyy"),
                TotalHours = b.TotalHours
            }).ToList();

            // Configuração do arquivo CSV para download.
            var fileName = $"Relatorio_Registros_{DateTime.Today.ToString("dd/MM/yyyy")}.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Id",
                "PID",
                "Nome",
                "Email",
                "Departamento",
                "Data Inicial",
                "Data Final",
                "Total de Horas",
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(listaCombinada, columnNames);

            // Retornar o arquivo CSV para download.
            return File(csvData, contentType, fileName);
        }

        [Authorize(Policy = "RequerPerfilAdmin")]
        [HttpPost]
        public async Task<IActionResult> ExportEmployeeDetail(int id)
        {
            // Registo de horas do funcionário selecionado.
            var employeeRecord = await _context.BiweeklyRecords
                .Include(b => b.Records)
                .Include(b => b.Department)
                .FirstOrDefaultAsync(b => b.BiweeklyRecordId == id);


            if (employeeRecord == null) return NotFound();

            // Lista dos registros de horas do funcionário.
            var records = employeeRecord.Records.Select(r => new
            {
                WBS = _context.WBS.FirstOrDefault(w => w.WBSId == r.WBSId).Code,
                Desc = _context.WBS.FirstOrDefault(w => w.WBSId == r.WBSId).Desc,
                Data = r.Data.ToString("dd/MM/yyyy"),
                Horas = r.Hours,

            }).ToList();

            // Configuração do arquivo CSV para download.
            var fileName = $"Relatorio_{employeeRecord.EmployeeName}_Q{employeeRecord.StartDate.ToString("dd/MM/yyyy")}.csv";
            var contentType = "text/csv";
            var columnNames = new List<string>
            {
                "Código",
                "WBS",
                "Data",
                "Horas"
            };

            // Escrever os dados em um arquivo CSV.
            var csvData = _csvService.WriteCSV(records, columnNames);

            // Retornar o arquivo CSV para download.
            return File(csvData, contentType, fileName);
        }

        public IActionResult PowerBiReport()
        {
            return View();
        }
    }
}
