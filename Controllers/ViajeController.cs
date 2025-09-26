using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;

namespace ProyetoSetilPF.Controllers
{
    

    public class ViajeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ViajeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        ////Coordinador asignado
        //private async Task<bool> EsCoordinadorAsignado(int viajeId)
        //{
        //    var userId = _userManager.GetUserId(User);

        //    if (User.IsInRole("Admin"))
        //        return true;

        //    return await _context.ViajeCoordinador
        //        .AnyAsync(vc => vc.ViajeId == viajeId && vc.Coordinador.UserId == userId);
        //}
        //// GET: Viaje
        //public async Task<IActionResult> Index(int pagina = 1)
        //{

        //    var userId = _userManager.GetUserId(User);
        //    IQueryable<Viaje> applicationDbContext;



        //    if (User.IsInRole("Admin"))
        //    {
        //        applicationDbContext = _context.Viaje;
        //    }
        //    else
        //    {
        //        // Solo viajes asignados al coordinador logueado
        //        applicationDbContext = _context.Viaje
        //            .Where(v => v.ViajeCoordinador.Any(vc => vc.Coordinador.UserId == userId));
        //    }

        //    var viajes = await applicationDbContext
        //        .Include(v => v.ViajeCiudad).ThenInclude(vc => vc.Ciudad)
        //        .Include(v => v.ViajeCoordinador).ThenInclude(vc => vc.Coordinador)
        //        .Include(v => v.ViajePasajero).ThenInclude(vp => vp.Pasajero)
        //        .Include(v => v.MovimientosViaje).ThenInclude(v => v.TipoMovimiento)
        //        .Include(v => v.DocumentosViaje)
        //        .ToListAsync();

        //    Paginador paginas = new Paginador
        //    {
        //        PaginaActual = pagina,
        //        RegistrosPorPagina = 5,
        //        TotalRegistros = applicationDbContext.Count()
        //    };

        //    var mostrarRegistros = applicationDbContext
        //        .Skip((pagina - 1) * paginas.RegistrosPorPagina)
        //        .Take(paginas.RegistrosPorPagina);

        //    ViajeVM datos = new ViajeVM
        //    {
        //        viaje = mostrarRegistros.ToList(),
        //        paginador = paginas
        //    };

        //    return View(datos);
        //}
        // Verifica si el coordinador logueado está asignado al viaje
        // Verifica si el coordinador tiene acceso al viaje
        private async Task<bool> EsCoordinadorAsignado(int viajeId)
        {
            var email = User.Identity.Name; // email logueado

            if (User.IsInRole("Admin") || User.IsInRole("Administracion"))
                return true;

            return await _context.ViajeCoordinador
                .Include(vc => vc.Coordinador)
                .AnyAsync(vc => vc.ViajeId == viajeId && vc.Coordinador.Email == email);
        }

        // GET: Viaje
        public async Task<IActionResult> Index(int pagina = 1)
        {
            IQueryable<Viaje> applicationDbContext;

            if (User.IsInRole("Admin") || User.IsInRole("Administracion"))
            {
                applicationDbContext = _context.Viaje;
            }
            else
            {
                var email = User.Identity.Name;

                // Solo viajes asignados al coordinador logueado
                applicationDbContext = _context.Viaje
                    .Where(v => v.ViajeCoordinador
                        .Any(vc => vc.Coordinador.Email == email));
            }

            var viajes = await applicationDbContext
                .Include(v => v.ViajeCiudad).ThenInclude(vc => vc.Ciudad)
                .Include(v => v.ViajeCoordinador).ThenInclude(vc => vc.Coordinador).ThenInclude(c => c.User)
                .Include(v => v.ViajePasajero).ThenInclude(vp => vp.Pasajero)
                .Include(v => v.MovimientosViaje).ThenInclude(v => v.TipoMovimiento)
                .Include(v => v.DocumentosViaje)
                .ToListAsync();

            Paginador paginas = new Paginador
            {
                PaginaActual = pagina,
                RegistrosPorPagina = 5,
                TotalRegistros = applicationDbContext.Count()
            };

            var mostrarRegistros = applicationDbContext
                .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                .Take(paginas.RegistrosPorPagina);

            ViajeVM datos = new ViajeVM
            {
                viaje = mostrarRegistros.ToList(),
                paginador = paginas
            };

            return View(datos);
        }



        // GET: Viaje/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            if (!await EsCoordinadorAsignado(id.Value))
                return Forbid();

            var viaje = await _context.Viaje.FirstOrDefaultAsync(m => m.Id == id);
            if (viaje == null) return NotFound();

            return View(viaje);
        }
        [Authorize(Roles = "Admin")]
        // GET: Viaje/Create
        public IActionResult Create()
        {

            ViewBag.Ciudad = new MultiSelectList(_context.Ciudad, "Id", "Descripcion");
            ViewBag.Pasajero = new MultiSelectList(_context.Pasajero, "Id", "Pasaporte");
            ViewBag.Coordinador = new MultiSelectList(_context.Coordinador, "Id", "Pasaporte");

            return View();
        }
        // POST: Viaje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,FechaIda,FechaVuelta,Balance")] Viaje viaje, int[] ciudadIds, int[] pasajeroIds, int[] coordinadorIds)
        {
            if (ModelState.IsValid)
            {

                _context.Add(viaje);
                await _context.SaveChangesAsync();
                if (ciudadIds != null)
                {
                    foreach (var ciudadId in ciudadIds)
                    {
                        _context.ViajeCiudad.Add(new ViajeCiudad
                        {
                            ViajeId = viaje.Id, // EF ya conoce el Id
                            CiudadId = ciudadId
                        });
                    }
                }

                if (pasajeroIds != null)
                {
                    foreach (var pasajeroId in pasajeroIds)
                    {
                        _context.ViajePasajero.Add(new ViajePasajero
                        {
                            ViajeId = viaje.Id,
                            PasajeroId = pasajeroId
                        });
                    }
                }

                if (coordinadorIds != null)
                {
                    foreach (var coordinadorId in coordinadorIds)
                    {
                        _context.ViajeCoordinador.Add(new ViajeCoordinador
                        {
                            ViajeId = viaje.Id,
                            CoordinadorId = coordinadorId
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Ciudad = new MultiSelectList(await _context.Ciudad.ToListAsync(), "Id", "Descripcion", ciudadIds);
            ViewBag.Pasajero = new MultiSelectList(await _context.Pasajero.ToListAsync(), "Id", "Dni", pasajeroIds);
            ViewBag.Coordinador = new MultiSelectList(await _context.Coordinador.ToListAsync(), "Id", "Dni", coordinadorIds);
            return View(viaje);
        }

        // GET: Viaje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viaje = await _context.Viaje.FindAsync(id);
            if (viaje == null)
            {
                return NotFound();
            }
            return View(viaje);
        }

        // POST: Viaje/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,FechaIda,FechaVuelta,Balance")] Viaje viaje)
        {
            if (id != viaje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viaje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ViajeExists(viaje.Id))
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
            return View(viaje);
        }

        // GET: Viaje/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viaje = await _context.Viaje
                .FirstOrDefaultAsync(m => m.Id == id);
            if (viaje == null)
            {
                return NotFound();
            }

            return View(viaje);
        }

        // POST: Viaje/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var viaje = await _context.Viaje.FindAsync(id);
            if (viaje != null)
            {
                _context.Viaje.Remove(viaje);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ViajeExists(int id)
        {
            return _context.Viaje.Any(e => e.Id == id);
        }
        // GET: Viaje/VerPasajeros/5
        public async Task<IActionResult> VerPasajeros(int? id)
        {
            if (id == null) return NotFound();

            var viaje = await _context.Viaje
                .Include(v => v.ViajePasajero)
                    .ThenInclude(vp => vp.Pasajero)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaje == null) return NotFound();

            return View(viaje.ViajePasajero.Select(vp => vp.Pasajero).ToList());
        }

        // GET: Viaje/HacerMovimiento/5
        public IActionResult HacerMovimiento(int viajeId)
        {
            // Creo un objeto MovimientoViaje con el ViajeId ya asignado
            var movimiento = new MovimientoViaje
            {
                ViajeId = viajeId,
                Fecha = DateTime.Now
            };

            // Paso la lista de tipos de movimiento (Ingreso/Egreso) a la vista
            ViewBag.TipoMovimientos = new SelectList(_context.TiposMovimiento, "Id", "Descripcion");
            return View(movimiento);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HacerMovimiento(MovimientoViaje movimiento)
        {
            if (ModelState.IsValid)
            {
                // Traigo el viaje correspondiente
                var viaje = _context.Viaje.FirstOrDefault(v => v.Id == movimiento.ViajeId);

                if (viaje != null)
                {
                    // Modifico el balance según el tipo de movimiento
                    if (movimiento.TipoMovimientoId == 1) // Ingreso
                        viaje.Balance += movimiento.Monto;
                    else if (movimiento.TipoMovimientoId == 2) // Egreso
                        viaje.Balance -= movimiento.Monto;

                    _context.Update(viaje); // aviso a EF que cambió el viaje
                }

                _context.MovimientosViaje.Add(movimiento); // agrego el movimiento
                _context.SaveChanges(); // guardo todo

                // REDIRECCIONO al Index de Viajes
                return RedirectToAction("Index", "Viaje");
            }

            // Si hay error, vuelvo a cargar la lista de tipos
            ViewBag.TipoMovimientos = new SelectList(_context.TiposMovimiento, "Id", "Descripcion");
            return View(movimiento);
        }
        public IActionResult VerMovimientos(int viajeId)
        {
            // Traigo el viaje con sus movimientos y los tipos
            var viaje = _context.Viaje
                                .Include(v => v.MovimientosViaje)
                                .ThenInclude(m => m.TipoMovimiento)
                                .FirstOrDefault(v => v.Id == viajeId);

            if (viaje == null)
                return NotFound();

            return View("VerMovimientos", viaje);
        }
        [HttpPost]
        public async Task<IActionResult> SubirPdf(int viajeId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "No se seleccionó ningún archivo.";
                return RedirectToAction("Index");
            }

            // Carpeta destino en wwwroot
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/Viajes", viajeId.ToString());

            // Crear carpeta si no existe
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            // Ruta completa del archivo
            var filePath = Path.Combine(carpeta, archivo.FileName);

            // Guardar archivo en disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Guardar registro en la base de datos
            var documento = new DocumentoViaje
            {
                NombreArchivo = archivo.FileName,
                RutaArchivo = $"/uploads/viajes/{viajeId}/{archivo.FileName}",
                ViajeId = viajeId
            };

            _context.DocumentosViaje.Add(documento);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "PDF subido correctamente.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DocumentoVista(int viajeId)
        {
            var viaje = await _context.Viaje
                .Include(v => v.DocumentosViaje)
                .FirstOrDefaultAsync(v => v.Id == viajeId);

            if (viaje == null)
                return NotFound();

            return View(viaje);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarDocumento(int documentoId)
        {
            // Busco el documento en la DB
            var documento = await _context.DocumentosViaje.FindAsync(documentoId);
            if (documento == null)
                return NotFound();

            // Ruta física del archivo en wwwroot
            var rutaFisica = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", documento.RutaArchivo.TrimStart('/').Replace("/", "\\"));
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica); // Borro el archivo del disco
            }

            // Borro el registro de la DB
            _context.DocumentosViaje.Remove(documento);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Archivo eliminado correctamente.";

            // Redirijo a la misma vista de documentos del viaje
            return RedirectToAction("DocumentoVista", new { viajeId = documento.ViajeId });
        }

    }



}

