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
    public class AgenciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgenciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Agencia
        public async Task<IActionResult> Index()
        {
            return View(await _context.Agencia.ToListAsync());
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

        // POST: Agencia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var agencia = await _context.Agencia.FindAsync(id);
            if (agencia != null)
            {
                _context.Agencia.Remove(agencia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgenciaExists(int id)
        {
            return _context.Agencia.Any(e => e.Id == id);
        }
    }
}
