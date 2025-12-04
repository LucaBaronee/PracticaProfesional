using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;

namespace ProyetoSetilPF.Controllers
{
    public class CiudadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment env;

        public CiudadController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            this.env = env;
        }





        public async Task<IActionResult> ImportarGenero(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Mensaje"] = "Error: No se ha proporcionado un archivo de Excel.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var package = new ExcelPackage(archivo.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var ciudades = new List<Ciudad>();

                    // Obtener nombres ya existentes en la base de datos
                    var nombresExistentes = _context.Ciudad.Select(c => c.Descripcion).ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Saltar encabezado
                    int startRow = worksheet.Dimension.Start.Row + 1;
                    int endRow = worksheet.Dimension.End.Row;

                    for (int row = startRow; row <= endRow; row++)
                    {
                        string descripcion = worksheet.Cells[row, 1].Text?.Trim();
                        string codigoPostalTexto = worksheet.Cells[row, 2].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(descripcion))
                            continue;

                        // Saltar si ya existe
                        if (nombresExistentes.Contains(descripcion))
                            continue;

                        int codigoPostal = 0;
                        int.TryParse(codigoPostalTexto, out codigoPostal);

                        var ciudad = new Ciudad
                        {
                            Descripcion = descripcion,
                            CodigoPostal = codigoPostal
                        };

                        ciudades.Add(ciudad);
                        nombresExistentes.Add(descripcion); // Añadir al HashSet para evitar duplicados dentro del mismo Excel
                    }

                    if (ciudades.Any())
                    {
                        _context.Ciudad.AddRange(ciudades);
                        await _context.SaveChangesAsync();
                        TempData["Mensaje"] = "Ciudades importadas exitosamente.";
                    }
                    else
                    {
                        TempData["Mensaje"] = "No se importaron ciudades nuevas (ya existían en la base de datos).";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error durante la importación. Verifica el archivo.";
                Console.WriteLine(ex);
            }

            return RedirectToAction("Index");
        }





        // GET: Ciuda
        public async Task<IActionResult> Index(string busqNombre,int pagina = 1)
        {
            Paginador paginas = new Paginador();
            paginas.PaginaActual = pagina;
            paginas.RegistrosPorPagina = 10;
            IQueryable<Ciudad> applicationDbContext = _context.Ciudad
                .Where(c => c.Activo);

            if (!string.IsNullOrEmpty(busqNombre))
            {
                applicationDbContext = applicationDbContext.Where(e => e.Descripcion.Contains(busqNombre));
                paginas.ValoresQueryString.Add("busquedaNombre", busqNombre);

            };

            paginas.TotalRegistros = applicationDbContext.Count();
            var mostrarRegistros = applicationDbContext
                            .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                            .Take(paginas.RegistrosPorPagina);
            CiudadVM datos = new CiudadVM()
            {
                busquedaNombre = busqNombre,
                ciudad = mostrarRegistros.ToList(),
                paginador = paginas,
                
            };
            return View(datos);
        }

        // GET: Ciudad/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudad = await _context.Ciudad
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ciudad == null)
            {
                return NotFound();
            }

            return View(ciudad);
        }

        // GET: Ciudad/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ciuda/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,CodigoPostal")] Ciudad ciudad)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ciudad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ciudad);
        }

        // GET: Ciuda/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudad = await _context.Ciudad.FindAsync(id);
            if (ciudad == null)
            {
                return NotFound();
            }
            return View(ciudad);
        }

        // POST: Ciuda/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,CodigoPostal")] Ciudad ciudad)
        {
            if (id != ciudad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ciudad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CiudadExists(ciudad.Id))
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
            return View(ciudad);
        }

        // GET: Ciuda/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ciudad = await _context.Ciudad
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ciudad == null)
            {
                return NotFound();
            }

            return View(ciudad);
        }

        // POST: Ciuda/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ciudad = await _context.Ciudad.FindAsync(id);
            if (ciudad != null)
            {
                _context.Ciudad.Remove(ciudad);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CiudadExists(int id)
        {
            return _context.Ciudad.Any(e => e.Id == id);
        }
    }
}
