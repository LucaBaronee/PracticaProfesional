using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProyetoSetilPF.Controllers
{
    public class PasajeroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _env;
        public PasajeroController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Pasajero
        public async Task<IActionResult> Index(string busqNombre, string busqApellido, string busqDni,int pagina =1,bool mostrarTodos= false)
        {
            Paginador paginas = new Paginador();
            paginas.PaginaActual = pagina;
            paginas.RegistrosPorPagina = 5;

            IQueryable<Pasajero> applicationDbContext = _context.Pasajero
                .Include(p => p.Sexo);
            if (!mostrarTodos)
            {
                applicationDbContext = applicationDbContext.Where(p => p.Activo);
            }

            if (!string.IsNullOrEmpty(busqNombre))
            {
                applicationDbContext = applicationDbContext.Where(e => e.Nombre.Contains(busqNombre));
                paginas.ValoresQueryString.Add("busquedaNombre", busqNombre);

            }
            if (!string.IsNullOrEmpty(busqApellido))
            {
                applicationDbContext = applicationDbContext.Where(e => e.Apellido.Contains(busqApellido));
                paginas.ValoresQueryString.Add("busquedaNombre", busqApellido);

            }
            if (!string.IsNullOrEmpty(busqDni))
            {
                applicationDbContext = applicationDbContext.Where(e => e.Pasaporte.Equals(busqDni));
                paginas.ValoresQueryString.Add("BusquedaDni", busqDni);
            }

            paginas.TotalRegistros = applicationDbContext.Count();
            var mostrarRegistros = applicationDbContext
                            .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                            .Take(paginas.RegistrosPorPagina);
            PasajeroVm datos = new PasajeroVm()
            {
                busquedaNombre = busqNombre,
                busquedaApellido = busqApellido,
                MostrarTodos = mostrarTodos,
                busquedaDni = busqDni,
                pasajero = mostrarRegistros.ToList(),
                paginador = paginas
            };

            
            return View(datos);
        }


        private async Task<int> ActualizarEstadoPasajerosAsync()
        {
            // Ahora - podés cambiar a UTC si tu app lo requiere
            var ahora = DateTime.Now;

            // 1) IDs de pasajeros que tienen viajes en curso o futuros
            var pasajeroIdsConViaje = await _context.ViajePasajero
                .Where(vp =>
                    // viaje en curso: FechaInicio <= ahora <= FechaFin
                    (vp.Viaje.FechaIda <= ahora && vp.Viaje.FechaVuelta >= ahora)
                    // o viaje futuro: FechaInicio > ahora
                    || (vp.Viaje.FechaIda > ahora)
                )
                .Select(vp => vp.PasajeroId)
                .Distinct()
                .ToListAsync();

            // 2) Traer sólo los pasajeros cuyo Activo debe cambiar (evita actualizar todos)
            var pasajerosParaActualizar = await _context.Pasajero
                .Where(p =>
                    (pasajeroIdsConViaje.Contains(p.Id) && !p.Activo)   // tienen viaje pero están en false -> poner true
                    || (!pasajeroIdsConViaje.Contains(p.Id) && p.Activo) // no tienen viaje pero están true -> poner false
                )
                .ToListAsync();

            // 3) Aplicar cambios en memoria
            foreach (var p in pasajerosParaActualizar)
            {
                p.Activo = pasajeroIdsConViaje.Contains(p.Id);
            }

            // 4) Guardar si hay cambios
            if (pasajerosParaActualizar.Any())
            {
                _context.UpdateRange(pasajerosParaActualizar);
                await _context.SaveChangesAsync();
            }

            // devolver cuántos registros se modificaron (para feedback)
            return pasajerosParaActualizar.Count;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstados()
        {
            int actualizados = await ActualizarEstadoPasajerosAsync();
            TempData["Mensaje"] = $"{actualizados} pasajeros actualizados.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Pasajero/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pasajero = await _context.Pasajero
                .Include(p => p.Sexo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pasajero == null)
            {
                return NotFound();
            }

            return View(pasajero);
        }

        // GET: Pasajero/Create
        public IActionResult Create(int? viajeId)
        {
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion");
            ViewBag.ViajeId = viajeId;
            return View();
        }

        // POST: Pasajero/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Agencia,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,FechaNacimiento,Telefono,Vencimiento")] Pasajero pasajero, int? viajeId)
        {
            if (ModelState.IsValid)
            {
                pasajero.FotoPasaporte =cargarFoto("");
                _context.Add(pasajero);
                await _context.SaveChangesAsync();
                if (viajeId.HasValue)
                {
                    return RedirectToAction("AgregarPasajero", "Viaje", new { viajeId = viajeId.Value });
                }


                return RedirectToAction(nameof(Index));
            }
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
            ViewBag.ViajeId = viajeId;
            return View(pasajero);
        }

        // GET: Pasajero/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pasajero = await _context.Pasajero.FindAsync(id);
            if (pasajero == null)
            {
                return NotFound();
            }
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
            return View(pasajero);
        }

        // POST: Pasajero/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Agencia,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,FechaNacimiento,Telefono,Vencimiento")] Pasajero pasajero)
        {
            if (id != pasajero.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pasajero.FotoPasaporte = cargarFoto(pasajero.FotoPasaporte);
                    _context.Update(pasajero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PasajeroExists(pasajero.Id))
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
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
            return View(pasajero);
        }

        // GET: Pasajero/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pasajero = await _context.Pasajero
                .Include(p => p.Sexo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pasajero == null)
            {
                return NotFound();
            }

            return View(pasajero);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pasajero = await _context.Pasajero.FindAsync(id);
            if (pasajero != null)
            {
                // En lugar de eliminar, lo marcamos como inactivo
                pasajero.Activo = false;
                _context.Update(pasajero);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PasajeroExists(int id)
        {
            return _context.Pasajero.Any(e => e.Id == id);
        }


        private string cargarFoto(string fotoAnterior)
        {
            var archivos = HttpContext.Request.Form.Files;
            if (archivos != null && archivos.Count > 0)
            {
                var archivoFoto = archivos[0];
                if (archivoFoto.Length > 0)
                {
                    var pathDestino = Path.Combine(_env.WebRootPath, "fotos");

                    // Si no existe la carpeta, la crea
                    if (!Directory.Exists(pathDestino))
                        Directory.CreateDirectory(pathDestino);

                    // Si había una foto anterior, la borro
                    if (!string.IsNullOrEmpty(fotoAnterior))
                    {
                        var rutaFotoAnterior = Path.Combine(pathDestino, fotoAnterior);
                        if (System.IO.File.Exists(rutaFotoAnterior))
                            System.IO.File.Delete(rutaFotoAnterior);
                    }

                    // Genero nuevo nombre único para la foto
                    var archivoDestino = Guid.NewGuid().ToString("N")
                                         + Path.GetExtension(archivoFoto.FileName);

                    // Guardo la nueva foto
                    using (var filestream = new FileStream(Path.Combine(pathDestino, archivoDestino), FileMode.Create))
                    {
                        archivoFoto.CopyTo(filestream);
                    }

                    return archivoDestino;
                }
            }
            return "";
        }
    }
}
