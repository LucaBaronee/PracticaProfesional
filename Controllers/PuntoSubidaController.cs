using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;

namespace ProyetoSetilPF.Controllers
{
    public class PuntoSubidaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PuntoSubidaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PuntoSubida
        public async Task<IActionResult> Index(int pagina = 1)
        {

            Paginador paginas = new Paginador();
            paginas.PaginaActual = pagina;
            paginas.RegistrosPorPagina = 5;

            IQueryable<PuntoSubida> applicationDbContext = _context.puntoSubida
                .Where(p => p.Activo);


            paginas.TotalRegistros = applicationDbContext.Count();
            var mostrarRegistros = applicationDbContext
                            .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                            .Take(paginas.RegistrosPorPagina);


            PuntoSubidaVM datos = new PuntoSubidaVM()
            {
              
                puntosubida = mostrarRegistros.ToList(),
                paginador = paginas,
            };

            return View(datos);


        }

        // GET: PuntoSubida/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var puntoSubida = await _context.puntoSubida
                .FirstOrDefaultAsync(m => m.Id == id);
            if (puntoSubida == null)
            {
                return NotFound();
            }

            return View(puntoSubida);
        }

        // GET: PuntoSubida/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PuntoSubida/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,Ciudad,Calle,CalleNumero")] PuntoSubida puntoSubida)
        {
            if (ModelState.IsValid)
            {
                _context.Add(puntoSubida);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(puntoSubida);
        }

        // GET: PuntoSubida/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var puntoSubida = await _context.puntoSubida.FindAsync(id);
            if (puntoSubida == null)
            {
                return NotFound();
            }
            return View(puntoSubida);
        }

        // POST: PuntoSubida/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,Ciudad,Calle,CalleNumero")] PuntoSubida puntoSubida)
        {
            if (id != puntoSubida.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(puntoSubida);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PuntoSubidaExists(puntoSubida.Id))
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
            return View(puntoSubida);
        }

        // GET: PuntoSubida/Delete/5
        // GET: PuntoSubida/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var puntoSubida = await _context.puntoSubida
                .FirstOrDefaultAsync(m => m.Id == id);

            if (puntoSubida == null)
            {
                return NotFound();
            }

            return View(puntoSubida);
        }

        // POST: PuntoSubida/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var puntoSubida = await _context.puntoSubida.FindAsync(id);

            if (puntoSubida != null)
            {
                // No borramos nada, solo desactivamos
                puntoSubida.Activo = false;
                _context.Update(puntoSubida);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Mostrar Puntos de Subida Eliminados
        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> Eliminadas()
        {
            var puntosEliminados = await _context.puntoSubida
                .Where(p => !p.Activo) // solo los desactivados
                .OrderBy(p => p.Descripcion)
                .ToListAsync();

            return View(puntosEliminados);
        }

        // POST: Reactivar Punto de Subida
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var punto = await _context.puntoSubida.FindAsync(id);
            if (punto == null)
                return NotFound();

            punto.Activo = true;
            _context.Update(punto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"El punto de subida '{punto.Descripcion}' fue reactivado.";
            return RedirectToAction(nameof(Eliminadas));
        }



        private bool PuntoSubidaExists(int id)
        {
            return _context.puntoSubida.Any(e => e.Id == id);
        }
    }
}