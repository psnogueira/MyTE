﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyTE.Data;
using MyTE.Data.Migrations;
using MyTE.Models;
using MyTE.Models.ViewModel;
using MyTE.Pagination;

using CsvHelper;
using CsvHelper.Configuration;
using MyTE.Models.Map;
using System.Text;

namespace MyTE.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            int pageSize = 5;
            ViewData["CurrentFilter"] = searchString;
            var department = from s in _context.Department
                      select s;

            if (!string.IsNullOrEmpty(searchString))
            {

                department = department.Where(s => s.Name.Contains(searchString));
            }

            var viewModel = new DepartmentViewModel
            {
                DepartmentList = await PaginatedList<Department>.CreateAsync(department.AsNoTracking(), pageNumber ?? 1, pageSize),
                Department = new Department(),
                CurrentFilter = searchString
            };
            return View(viewModel);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DepartmentId,Name")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Departamento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DepartmentId,Name")] Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage2"] = "Departamento editado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var department = await _context.Department
        //        .FirstOrDefaultAsync(m => m.DepartmentId == id);
        //    if (department == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(department);
        //}

        // POST: Departments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Department.FindAsync(id);
            if (department != null)
            {
                _context.Department.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.DepartmentId == id);
        }


        [HttpPost]
        public IActionResult ExportToCSV()
        {
            // Lista de Departamentos do banco de dados.
            var departments = _context.Department.ToList();

            // Criação do arquivo CSV pelo MemoryStream.
            // Ele serve para ler e gravar dados em memória.
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    // Registro de dados no arquivo CSV.
                    csvWriter.Context.RegisterClassMap<DepartmentMap>(); // Mapeamento de dados.
                    csvWriter.WriteHeader<Department>();                 // Cabeçalho do arquivo.
                    csvWriter.NextRecord();                              // Ir para próxima linha (obrigatório depois de .WriteHeader<>()).
                    csvWriter.WriteRecords(departments);                 // Registros dos dados.

                    csvWriter.Flush();          // Limpa o buffer de gravação.
                    memoryStream.Position = 0;  // Posiciona o ponteiro no início do arquivo.
                }

                // Retorna o arquivo CSV para download.
                // O arquivo é gerado em memória e não é salvo no servidor.
                return File(memoryStream.ToArray(), "text/csv", "departments.csv");
            }
        }
    }
}
