using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.Controllers
{
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Coordinador
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Coordinador.Include(c => c.Sexo);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Coordinador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinador = await _context.Coordinador
                .Include(c => c.Sexo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (coordinador == null)
            {
                return NotFound();
            }

            return View(coordinador);
        }

        // GET: Coordinador/Create
        public IActionResult Create()
        {
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion");
            return View();
        }

        // POST: Coordinador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,Vencimiento,FechaNacimiento,Telefono,Email")] Coordinador coordinador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(coordinador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", coordinador.SexoId);
            return View(coordinador);
        }

        // GET: Coordinador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinador = await _context.Coordinador.FindAsync(id);
            if (coordinador == null)
            {
                return NotFound();
            }
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", coordinador.SexoId);
            return View(coordinador);
        }

        // POST: Coordinador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,Vencimiento,FechaNacimiento,Telefono,Email")] Coordinador coordinador)
        {
            if (id != coordinador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(coordinador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CoordinadorExists(coordinador.Id))
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
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", coordinador.SexoId);
            return View(coordinador);
        }

        // GET: Coordinador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coordinador = await _context.Coordinador
                .Include(c => c.Sexo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (coordinador == null)
            {
                return NotFound();
            }

            return View(coordinador);
        }

        // POST: Coordinador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coordinador = await _context.Coordinador.FindAsync(id);
            if (coordinador != null)
            {
                _context.Coordinador.Remove(coordinador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CoordinadorExists(int id)
        {
            return _context.Coordinador.Any(e => e.Id == id);
        }
    }
}
