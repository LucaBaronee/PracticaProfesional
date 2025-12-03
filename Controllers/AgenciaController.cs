using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Data.Migrations;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;

namespace ProyetoSetilPF.Controllers
{
    public class AgenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgenciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Agencia
        public async Task<IActionResult> Index(string busqNombre, int pagina = 1)
        {
            Paginador paginas = new Paginador
            {
                PaginaActual = pagina,
                RegistrosPorPagina = 5
            };

            // Solo agencias activas
            IQueryable<Agencia> query = _context.Agencia
                .Where(a => a.Activo);

            // Filtro por nombre
            if (!string.IsNullOrEmpty(busqNombre))
            {
                query = query.Where(a => a.Nombre.Contains(busqNombre));
                paginas.ValoresQueryString.Add("busqNombre", busqNombre);
            }

            // Total registros
            paginas.TotalRegistros = query.Count();

            // Paginación
            var mostrarRegistros = query
                .OrderBy(a => a.Nombre) // opcional, orden por nombre
                .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                .Take(paginas.RegistrosPorPagina)
                .ToList();

            // ViewModel
            AgenciaVM datos = new AgenciaVM
            {
                busquedaNombre = busqNombre,
                agencia = mostrarRegistros,
                paginador = paginas
            };

            return View(datos);
        }

        // GET: Agencia/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agencia = await _context.Agencia
                .FirstOrDefaultAsync(m => m.Id == id);
            if (agencia == null)
            {
                return NotFound();
            }

            return View(agencia);
        }

        // GET: Agencia/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Agencia/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre")] Agencia agencia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(agencia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(agencia);
        }

        // GET: Agencia/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agencia = await _context.Agencia.FindAsync(id);
            if (agencia == null)
            {
                return NotFound();
            }
            return View(agencia);
        }

        // POST: Agencia/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Agencia agencia)
        {
            if (id != agencia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(agencia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgenciaExists(agencia.Id))
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
            return View(agencia);
        }

        // GET: Agencia/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agencia = await _context.Agencia
                .FirstOrDefaultAsync(m => m.Id == id);
            if (agencia == null)
            {
                return NotFound();
            }

            return View(agencia);
        }




        // GET: Mostrar Agencias Eliminadas
        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> Eliminadas()
        {
            var agenciasEliminadas = await _context.Agencia
                .Where(a => !a.Activo) // solo las eliminadas
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return View(agenciasEliminadas);
        }

        // POST: Reactivar Agencia
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var agencia = await _context.Agencia.FindAsync(id);
            if (agencia == null)
                return NotFound();

            agencia.Activo = true;
            _context.Update(agencia);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"La agencia '{agencia.Nombre}' fue reactivada.";
            return RedirectToAction(nameof(Eliminadas));
        }





        // POST: Agencia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var agencia = await _context.Agencia.FindAsync(id);

            if (agencia != null)
            {
                // No borramos nada, solo desactivamos
                agencia.Activo = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AgenciaExists(int id)
        {
            return _context.Agencia.Any(e => e.Id == id);
        }
    }
}
