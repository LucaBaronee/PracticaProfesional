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
        [Authorize(Roles = "Admin,Administracion,Coordinador")]
        // GET: Pasajero
        public async Task<IActionResult> Index(string busqNombre, string busqApellido, string busqDni,int pagina =1,bool mostrarTodos= false)
        {
            Paginador paginas = new Paginador();
            paginas.PaginaActual = pagina;
            paginas.RegistrosPorPagina = 5;

            IQueryable<Pasajero> applicationDbContext = _context.Pasajero
                .Include(p => p.Sexo)
                .Where(p=> p.Activo);

            if (mostrarTodos)
            {
                applicationDbContext = applicationDbContext.Where(p => p.EnViaje);
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
                busquedaDni = busqDni,
                pasajero = mostrarRegistros.ToList(),
                paginador = paginas,
                MostrarTodos = mostrarTodos
            };

            
            return View(datos);
        }
        [Authorize(Roles = "Admin,Administracion")]

        private async Task<int> ActualizarEstadoPasajerosAsync()
        {
            var ahora = DateTime.Now;

            // 1️⃣ Obtener IDs de pasajeros con viajes en curso o futuros
            var pasajeroIdsConViaje = await _context.ViajePasajero
                .Where(vp =>
                    (vp.Viaje.FechaIda <= ahora && vp.Viaje.FechaVuelta >= ahora) // viaje en curso
                    || (vp.Viaje.FechaIda > ahora)                                // viaje futuro
                )
                .Select(vp => vp.PasajeroId)
                .Distinct()
                .ToListAsync();

            // 2️⃣ Traer solo pasajeros activos cuyo estado EnViaje debe cambiar
            var pasajerosParaActualizar = await _context.Pasajero
                .Where(p => p.Activo && ( // solo pasajeros no eliminados
                    (pasajeroIdsConViaje.Contains(p.Id) && !p.EnViaje) ||   // debería estar en viaje y no lo está
                    (!pasajeroIdsConViaje.Contains(p.Id) && p.EnViaje)      // no debería estar en viaje pero lo está
                ))
                .ToListAsync();

            // 3️⃣ Actualizar bandera EnViaje
            foreach (var p in pasajerosParaActualizar)
            {
                p.EnViaje = pasajeroIdsConViaje.Contains(p.Id);
            }

            // 4️⃣ Guardar si hubo cambios
            if (pasajerosParaActualizar.Any())
            {
                _context.UpdateRange(pasajerosParaActualizar);
                await _context.SaveChangesAsync();
            }

            return pasajerosParaActualizar.Count;
        }

        public async Task<IActionResult> ViajesRealizados(int id)
        {
            var viajes = await _context.ViajePasajero
                .Where(vp => vp.PasajeroId == id)
                .Select(vp => vp.Viaje)
                .ToListAsync();

            ViewBag.PasajeroId = id;

            return View(viajes);
        }




        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstados()
        {
            int actualizados = await ActualizarEstadoPasajerosAsync();
            TempData["Mensaje"] = $"{actualizados} pasajeros actualizados.";
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin,Administracion,Coordinador")]
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
        [Authorize(Roles = "Admin,Administracion")]
        // GET: Pasajero/Create
        public IActionResult Create(int? viajeId)
        {
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion");
            ViewBag.ViajeId = viajeId;
            return View();
        }
        [Authorize(Roles = "Admin,Administracion")]
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
        [Authorize(Roles = "Admin,Administracion")]
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

        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,FechaNacimiento,Telefono,Vencimiento")] Pasajero pasajero)
        {
            if (id != pasajero.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var pasajeroDb = await _context.Pasajero.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (pasajeroDb == null) return NotFound();

                    // Mantener la foto anterior si no se subió ninguna
                    var nuevaFoto = HttpContext.Request.Form.Files.FirstOrDefault();
                    if (nuevaFoto != null && nuevaFoto.Length > 0)
                    {
                        pasajero.FotoPasaporte = cargarFoto(pasajeroDb.FotoPasaporte);
                    }
                    else
                    {
                        pasajero.FotoPasaporte = pasajeroDb.FotoPasaporte;
                    }

                    // Mantener la bandera EnViaje y Activo
                    pasajero.EnViaje = pasajeroDb.EnViaje;
                    pasajero.Activo = pasajeroDb.Activo;

                    _context.Update(pasajero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Pasajero.Any(e => e.Id == pasajero.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
            return View(pasajero);
        }



        [Authorize(Roles = "Admin,Administracion")]
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
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🔹 Cargar pasajero con sus viajes asociados
            var pasajero = await _context.Pasajero
                .Include(p => p.ViajePasajero)
                .ThenInclude(vp => vp.Viaje)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pasajero == null)
                return NotFound();
            var hoy = DateTime.Today;
            // 🔹 Filtrar solo los viajes futuros (FechaIda > ahora)
            var viajesFuturos = pasajero.ViajePasajero
                .Where(vp => vp.Viaje.FechaIda >= hoy)
                .ToList();

            // 🔹 Eliminar las relaciones con viajes futuros
            if (viajesFuturos.Any())
            {
                _context.ViajePasajero.RemoveRange(viajesFuturos);
            }

            // 🔹 Baja lógica del pasajero
            pasajero.Activo = false;
            pasajero.EnViaje = false;

            // 🔹 Guardar cambios de manera atómica
            try
            {
                _context.Update(pasajero);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Opcional: registrar el error y mostrar mensaje
                TempData["Error"] = "Ocurrió un error al eliminar el pasajero: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"El pasajero '{pasajero.Nombre} {pasajero.Apellido}' fue eliminado y desvinculado de {viajesFuturos.Count} viaje(s) futuro(s).";

            return RedirectToAction(nameof(Index));
        }

        private bool PasajeroExists(int id)
        {
            return _context.Pasajero.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Eliminados()
        {
            var pasajerosEliminados = await _context.Pasajero
                .Include(p => p.Sexo)
                .Where(p => !p.Activo) // solo los eliminados
                .OrderBy(p => p.Apellido)
                .ToListAsync();

            return View(pasajerosEliminados);
        }
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var pasajero = await _context.Pasajero.FindAsync(id);
            if (pasajero == null)
                return NotFound();

            pasajero.Activo = true;
            _context.Update(pasajero);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"El pasajero '{pasajero.Nombre} {pasajero.Apellido}' fue reactivado.";
            return RedirectToAction(nameof(Eliminados));
        }
        [Authorize(Roles = "Admin,Administracion")]
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
