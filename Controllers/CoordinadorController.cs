using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;

namespace ProyetoSetilPF.Controllers
{
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _env;

        public CoordinadorController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> Index(string busqNombre, string busqApellido, string busqDni, int pagina = 1, bool mostrarTodos = false)
        {
            Paginador paginas = new Paginador();
            paginas.PaginaActual = pagina;
            paginas.RegistrosPorPagina = 5;

            IQueryable<Coordinador> applicationDbContext = _context.Coordinador
                 .Include(p => p.Sexo)
                 .Where(p => p.Activo);

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

            CoordinadorVM datos = new CoordinadorVM()
            {
                busquedaNombre = busqNombre,
                busquedaApellido = busqApellido,
                busquedaDni = busqDni,
                coordinador = mostrarRegistros.ToList(),
                paginador = paginas,
                MostrarTodos = mostrarTodos
            };

            return View(datos);
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
                coordinador.FotoPasaporte = cargarFoto("");
                _context.Add(coordinador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", coordinador.SexoId);
            return View(coordinador);
        }




        private async Task<int> ActualizarEstadoCoordinadorAsync()
        {
            var ahora = DateTime.Now;

            // 1️⃣ Obtener IDs de pasajeros con viajes en curso o futuros
            var coordinadorIdsConViaje = await _context.ViajeCoordinador
                .Where(vp =>
                    (vp.Viaje.FechaIda <= ahora && vp.Viaje.FechaVuelta >= ahora) // viaje en curso
                    || (vp.Viaje.FechaIda > ahora)                                // viaje futuro
                )
                .Select(vp => vp.CoordinadorId)
                .Distinct()
                .ToListAsync();

            // 2️⃣ Traer solo pasajeros activos cuyo estado EnViaje debe cambiar
            var pasajerosParaActualizar = await _context.Coordinador
                .Where(p => p.Activo && ( // solo pasajeros no eliminados
                    (coordinadorIdsConViaje.Contains(p.Id) && !p.EnViaje) ||   // debería estar en viaje y no lo está
                    (!coordinadorIdsConViaje.Contains(p.Id) && p.EnViaje)      // no debería estar en viaje pero lo está
                ))
                .ToListAsync();

            // 3️⃣ Actualizar bandera EnViaje
            foreach (var p in pasajerosParaActualizar)
            {
                p.EnViaje = coordinadorIdsConViaje.Contains(p.Id);
            }

            // 4️⃣ Guardar si hubo cambios
            if (pasajerosParaActualizar.Any())
            {
                _context.UpdateRange(pasajerosParaActualizar);
                await _context.SaveChangesAsync();
            }

            return pasajerosParaActualizar.Count;
        }
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstados()
        {
            int actualizados = await ActualizarEstadoCoordinadorAsync();
            TempData["Mensaje"] = $"{actualizados} pasajeros actualizados.";
            return RedirectToAction(nameof(Index));
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
                    // Obtener la entidad original de la base para mantener propiedades que no se editan
                    var coordinadorDb = await _context.Coordinador.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (coordinadorDb == null) return NotFound();

                    // Mantener la foto anterior si no se subió ninguna nueva
                    var nuevaFoto = HttpContext.Request.Form.Files.FirstOrDefault();
                    if (nuevaFoto != null && nuevaFoto.Length > 0)
                    {
                        coordinador.FotoPasaporte = cargarFoto(coordinadorDb.FotoPasaporte);
                    }
                    else
                    {
                        coordinador.FotoPasaporte = coordinadorDb.FotoPasaporte;
                    }

                    // Mantener las banderas o propiedades que no se modifican desde el formulario
                    coordinador.Activo = coordinadorDb.Activo;
                    coordinador.EnViaje = coordinadorDb.EnViaje; // si aplica

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

        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🔹 Cargar pasajero con sus viajes asociados
            var coordinador = await _context.Coordinador
                .Include(p => p.ViajeCoordinador)
                .ThenInclude(vp => vp.Viaje)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (coordinador == null)
                return NotFound();
            var hoy = DateTime.Today;
            // 🔹 Filtrar solo los viajes futuros (FechaIda > ahora)
            var viajesFuturos = coordinador.ViajeCoordinador
                .Where(vp => vp.Viaje.FechaIda >= hoy)
                .ToList();

            // 🔹 Eliminar las relaciones con viajes futuros
            if (viajesFuturos.Any())
            {
                _context.ViajeCoordinador.RemoveRange(viajesFuturos);
            }

            // 🔹 Baja lógica del pasajero
            coordinador.Activo = false;
            coordinador.EnViaje = false;

            // 🔹 Guardar cambios de manera atómica
            try
            {
                _context.Update(coordinador);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Opcional: registrar el error y mostrar mensaje
                TempData["Error"] = "Ocurrió un error al eliminar el coordinador: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = $"El coordinador '{coordinador.Nombre} {coordinador.Apellido}' fue eliminado y desvinculado de {viajesFuturos.Count} viaje(s) futuro(s).";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Eliminados()
        {
            var coordinadorEliminados = await _context.Coordinador
                .Include(p => p.Sexo)
                .Where(p => !p.Activo) // solo los eliminados
                .OrderBy(p => p.Apellido)
                .ToListAsync();

            return View(coordinadorEliminados);
        }
        public async Task<IActionResult> Reactivar(int id)
        {
            var coordinador = await _context.Coordinador.FindAsync(id);
            if (coordinador == null)
                return NotFound();

            coordinador.Activo = true;
            _context.Update(coordinador);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"El pasajero '{coordinador.Nombre} {coordinador.Apellido}' fue reactivado.";
            return RedirectToAction(nameof(Eliminados));
        }
        private bool CoordinadorExists(int id)
        {
            return _context.Coordinador.Any(e => e.Id == id);
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
