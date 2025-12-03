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

       
        private async Task<bool> EsCoordinadorAsignado(int viajeId)
        {
            var email = User.Identity.Name; // email logueado

            if (User.IsInRole("Admin") || User.IsInRole("Administracion"))
                return true;

            return await _context.ViajeCoordinador
                .Include(vc => vc.Coordinador)
                .AnyAsync(vc => vc.ViajeId == viajeId && vc.Coordinador.Email == email);
        }
        [Authorize]
        // GET: Viaje
        public async Task<IActionResult> Index(string busquedaNombre, bool ordenDesc = true, int pagina = 1)
        {
            IQueryable<Viaje> applicationDbContext;

            var ahora = DateTime.Now;

            // 🔹 Filtro por rol
            if (User.IsInRole("Admin") || User.IsInRole("Administracion"))
            {
                // Solo viajes futuros o en curso
                applicationDbContext = _context.Viaje
                    .Where(v => v.FechaVuelta >= ahora);
            }
            else
            {
                var email = User.Identity.Name;

                // Solo viajes asignados al coordinador logueado y que aún no terminaron
                applicationDbContext = _context.Viaje
                    .Where(v => v.FechaVuelta >= ahora &&
                                v.ViajeCoordinador.Any(vc => vc.Coordinador.Email == email));
            }

            // 🔹 Filtro por nombre de viaje
            if (!string.IsNullOrEmpty(busquedaNombre))
            {
                applicationDbContext = applicationDbContext
                    .Where(v => v.Descripcion.Contains(busquedaNombre));
            }

            // 🔹 Incluye relaciones
            applicationDbContext = applicationDbContext
                .Include(v => v.ViajeCiudad).ThenInclude(vc => vc.Ciudad)
                .Include(v => v.ViajeCoordinador).ThenInclude(vc => vc.Coordinador).ThenInclude(c => c.User)
                .Include(v => v.ViajePasajero).ThenInclude(vp => vp.Pasajero)
                .Include(v => v.MovimientosViaje).ThenInclude(v => v.TipoMovimiento)
                .Include(v => v.DocumentosViaje)
                .Include(v=>v.Moneda);

            // 🔹 Orden por fecha de ida
            applicationDbContext = ordenDesc
                ? applicationDbContext.OrderByDescending(v => v.FechaIda)
                : applicationDbContext.OrderBy(v => v.FechaIda);

            // 🔹 Paginación
            var totalRegistros = await applicationDbContext.CountAsync();

            Paginador paginas = new Paginador
            {
                PaginaActual = pagina,
                RegistrosPorPagina = 2,
                TotalRegistros = totalRegistros
            };

            var mostrarRegistros = await applicationDbContext
                .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                .Take(paginas.RegistrosPorPagina)
                .ToListAsync();

            ViajeVM datos = new ViajeVM
            {
                viaje = mostrarRegistros,
                paginador = paginas,
                busquedaNombre = busquedaNombre,
                ordenDesc = ordenDesc
            };

            return View(datos);
        }


        // 🔸 NUEVO: HISTORIAL DE VIAJES
        public async Task<IActionResult> Historial(int pagina = 1)
        {
            IQueryable<Viaje> applicationDbContext;

            var ahora = DateTime.Now;

            // 🔹 Filtro por rol
            if (User.IsInRole("Admin") || User.IsInRole("Administracion"))
            {
                // Solo viajes finalizados
                applicationDbContext = _context.Viaje
                    .Where(v => v.FechaVuelta < ahora);
            }
            else
            {
                var email = User.Identity.Name;

                // Solo viajes del coordinador logueado y ya terminados
                applicationDbContext = _context.Viaje
                    .Where(v => v.FechaVuelta < ahora &&
                                v.ViajeCoordinador.Any(vc => vc.Coordinador.Email == email));
            }

            // 🔹 Incluye relaciones
            applicationDbContext = applicationDbContext
                .Include(v => v.ViajeCiudad).ThenInclude(vc => vc.Ciudad)
                .Include(v => v.ViajeCoordinador).ThenInclude(vc => vc.Coordinador).ThenInclude(c => c.User)
                .Include(v => v.ViajePasajero).ThenInclude(vp => vp.Pasajero)
                .Include(v => v.MovimientosViaje).ThenInclude(v => v.TipoMovimiento)
                .Include(v => v.DocumentosViaje)
                .OrderByDescending(v => v.FechaVuelta);

            // 🔹 Paginación
            var totalRegistros = await applicationDbContext.CountAsync();

            Paginador paginas = new Paginador
            {
                PaginaActual = pagina,
                RegistrosPorPagina = 5,
                TotalRegistros = totalRegistros
            };

            var mostrarRegistros = await applicationDbContext
                .Skip((pagina - 1) * paginas.RegistrosPorPagina)
                .Take(paginas.RegistrosPorPagina)
                .ToListAsync();

            ViajeVM datos = new ViajeVM
            {
                viaje = mostrarRegistros,
                paginador = paginas
            };

            // 🔹 Usamos una vista distinta para el historial
            return View("Historial", datos);
        }

        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> DetallesCompleto(int id)
        {
            var viaje = await _context.Viaje
                .Include(v => v.ViajeCiudad)
                    .ThenInclude(vc => vc.Ciudad)
                .Include(v => v.ViajeCoordinador)
                    .ThenInclude(vc => vc.Coordinador)
                .Include(v => v.ViajePasajero)
                    .ThenInclude(vp => vp.Pasajero)
                .Include(v => v.ViajePasajero)
                    .ThenInclude(vp => vp.Agencia)
                .Include(v => v.MovimientosViaje)
                    .ThenInclude(m => m.TipoMovimiento)
                .Include(v => v.DocumentosViaje)
                .Include(v=> v.Moneda)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaje == null)
                return NotFound();

            return View(viaje);
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


        [Authorize(Roles = "Admin,Administracion")]
        // GET: Viaje/Create
        public IActionResult Create()
        {
            ViewBag.MonedaId = new SelectList(_context.Moneda, "Id", "Nombre");
            ViewBag.Ciudad = new MultiSelectList(_context.Ciudad, "Id", "Descripcion");
            ViewBag.Pasajero = new MultiSelectList(_context.Pasajero, "Id", "Pasaporte");
            ViewBag.Coordinador = new MultiSelectList(_context.Coordinador, "Id", "Pasaporte");


            return View();
        }
        // POST: Viaje/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion,FechaIda,FechaVuelta,Balance,MonedaId")] Viaje viaje, int[] ciudadIds, int[] pasajeroIds, int[] coordinadorIds)
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
            var ciudades = await _context.Ciudad.ToListAsync() ?? new List<Ciudad>();
            var pasajeros = await _context.Pasajero.ToListAsync() ?? new List<Pasajero>();
            var coordinadores = await _context.Coordinador.ToListAsync() ?? new List<Coordinador>();
            ViewBag.MonedaId = new SelectList(_context.Moneda, "Id", "Nombre");
            ViewBag.Ciudad = new MultiSelectList(await _context.Ciudad.ToListAsync(), "Id", "Descripcion", ciudadIds);
            ViewBag.Pasajero = new MultiSelectList(await _context.Pasajero.ToListAsync(), "Id", "Pasaporte", pasajeroIds);
            ViewBag.Coordinador = new MultiSelectList(await _context.Coordinador.ToListAsync(), "Id", "Pasaporte", coordinadorIds);
            return View(viaje);
        }
        [Authorize(Roles = "Admin,Administracion")]
        // GET: Viaje/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.MonedaId = new SelectList(_context.Moneda, "Id", "Nombre");
            var viaje = await _context.Viaje.FindAsync(id);
            if (viaje == null)
            {
                return NotFound();
            }
            return View(viaje);
        }
        
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,FechaIda,FechaVuelta,MonedaId")] Viaje viaje)
        {
            if (id != viaje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 🔒 Obtener el viaje original desde la base de datos
                    var viajeExistente = await _context.Viaje.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                    if (viajeExistente == null)
                    {
                        return NotFound();
                    }

                    // 🔁 Mantener el balance original (no se modifica)
                    viaje.Balance = viajeExistente.Balance;

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
            ViewBag.MonedaId = new SelectList(_context.Moneda, "Id", "Nombre");
            return View(viaje);
        }




        [Authorize(Roles = "Admin,Administracion")]
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
        [Authorize(Roles = "Admin,Administracion")]
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

        [Authorize]
        public async Task<IActionResult> VerPasajeros(int? id)
        {
            if (id == null) return NotFound();

            var viaje = await _context.Viaje
                .Include(v => v.ViajePasajero)
                    .ThenInclude(vp => vp.Pasajero)
                .Include(v => v.ViajePasajero)
                    .ThenInclude(vp => vp.Agencia)
                    .Include(v=>v.ViajePasajero)
                    .ThenInclude(v=>v.PuntoSubida)// 🔹 incluir agencia
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaje == null) return NotFound();

            // Pasajeros ya asignados
            var asignadosIds = viaje.ViajePasajero.Select(vp => vp.PasajeroId).ToList();

            // Pasajeros disponibles para agregar
            var disponibles = await _context.Pasajero
                .Where(p => !asignadosIds.Contains(p.Id))
                .ToListAsync();

            ViewBag.ViajeId = id;
            ViewBag.PasajerosDisponibles = new SelectList(disponibles, "Id", "Nombre");

            // 🔹 Retornar la lista de ViajePasajero completos
            return View(viaje.ViajePasajero.ToList());
        }
        // GET: Viaje/HacerMovimiento/5
        [Authorize(Roles = "Admin,Coordinador")]

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
        [Authorize(Roles = "Admin,Coordinador")]


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HacerMovimiento(MovimientoViaje movimiento)
        {
            if (ModelState.IsValid)
            {
                movimiento.Fecha = DateTime.Today;
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
        [Authorize]

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
        [Authorize(Roles = "Admin,Administracion")]

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
        [Authorize]
        public async Task<IActionResult> DocumentoVista(int viajeId)
        {
            var viaje = await _context.Viaje
                .Include(v => v.DocumentosViaje)
                .FirstOrDefaultAsync(v => v.Id == viajeId);

            if (viaje == null)
                return NotFound();

            return View(viaje);
        }
        [Authorize(Roles = "Admin,Administracion")]

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
        [Authorize]

        public async Task<IActionResult> VerCoordinador(int id)
        {
            var viaje = await _context.Viaje
                .Include(v => v.ViajeCoordinador)
                .ThenInclude(vc => vc.Coordinador)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaje == null) return NotFound();

            // Coordinadores ya asignados
            var asignadosIds = viaje.ViajeCoordinador.Select(vc => vc.CoordinadorId).ToList();

            // Coordinadores disponibles para agregar
            var disponibles = await _context.Coordinador
                .Where(c => !asignadosIds.Contains(c.Id))
                .ToListAsync();

            // Pasamos la lista a la vista
            ViewBag.ViajeId = id;
            ViewBag.CoordinadoresDisponibles = new SelectList(disponibles, "Id", "Nombre");

            return View(viaje.ViajeCoordinador.Select(vc => vc.Coordinador).ToList());
        }
        [Authorize]
        public async Task<IActionResult> VerCiudad(int id)
        {
            var viaje = await _context.Viaje
                .Include(v => v.ViajeCiudad)
                .ThenInclude(vc => vc.Ciudad)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaje == null) return NotFound();

            var ciudades = viaje.ViajeCiudad.Select(vc => vc.Ciudad).ToList();
            return View(ciudades);
        }
        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        [Authorize(Roles = "Admin,Administracion")]
        

        [HttpPost]
        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> AgregarCoordinador(int viajeId, int coordinadorId)
        {
            var existe = await _context.ViajeCoordinador
                .AnyAsync(vc => vc.ViajeId == viajeId && vc.CoordinadorId == coordinadorId);

            if (!existe)
            {
                var vc = new ViajeCoordinador
                {
                    ViajeId = viajeId,
                    CoordinadorId = coordinadorId
                };
                _context.ViajeCoordinador.Add(vc);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VerCoordinador", new { id = viajeId });
        }

      

     
        public async Task<IActionResult> AgregarPasajero(
    int viajeId,
    string busqueda = "",
    int pagina = 1,
    int registrosPorPagina = 10)
        {
            var viaje = await _context.Viaje.FindAsync(viajeId);
            if (viaje == null) return NotFound();

            // IDs ya asignados
            var pasajerosAsignadosIds = _context.ViajePasajero
                .Where(vp => vp.ViajeId == viajeId)
                .Select(vp => vp.PasajeroId)
                .ToList();

            // Base query
            var query = _context.Pasajero
                .Where(p => !pasajerosAsignadosIds.Contains(p.Id));

            // Filtro
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(busqueda) ||
                    p.Apellido.Contains(busqueda) ||
                    p.Pasaporte.Contains(busqueda));
            }

            int totalRegistros = await query.CountAsync();

            var pasajeros = await query
                .OrderBy(p => p.Apellido)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            // Armar view model
            var vm = new PasajeroVm
            {
                pasajero = pasajeros,
                busquedaNombre = busqueda,
                paginador = new Paginador
                {
                    PaginaActual = pagina,
                    TotalRegistros = totalRegistros,
                    RegistrosPorPagina = registrosPorPagina,
                    ValoresQueryString = new Dictionary<string, string>
            {
                {"viajeId", viajeId.ToString()},
                {"busqueda", busqueda }
            }
                }
            };

            // Viewbags
            ViewBag.Agencias = _context.Agencia
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre })
                .ToList();

            ViewBag.PuntoSubida = _context.puntoSubida
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Descripcion })
                .ToList();

            ViewBag.ViajeId = viajeId;

            return View(vm);
        }




        [Authorize(Roles = "Admin,Administracion")]
        [HttpPost]
        public async Task<IActionResult> AgregarPasajero(int viajeId, int pasajeroId, int agenciaId, int puntoSubida)
        {
            // Validar que no exista ya
            var existe = await _context.ViajePasajero
                .AnyAsync(vp => vp.ViajeId == viajeId && vp.PasajeroId == pasajeroId);

            if (!existe)
            {
                // Validar agencia
                var agencia = await _context.Agencia.FindAsync(agenciaId);
                if (agencia == null)
                {
                    ModelState.AddModelError("", "La agencia seleccionada no existe.");
                    return RedirectToAction("AgregarPasajero", new { viajeId });
                }

                // Validar punto de subida
                var punto = await _context.puntoSubida.FindAsync(puntoSubida);
                if (punto == null)
                {
                    ModelState.AddModelError("", "El punto de subida seleccionado no existe.");
                    return RedirectToAction("AgregarPasajero", new { viajeId });
                }

                _context.ViajePasajero.Add(new ViajePasajero
                {
                    ViajeId = viajeId,
                    PasajeroId = pasajeroId,
                    AgenciaId = agenciaId,
                    PuntoSubidaId = puntoSubida
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VerPasajeros", new { id = viajeId });
        }

        //[Authorize(Roles = "Admin,Administracion")]
        //public async Task<IActionResult> AgregarCoordinador(int viajeId, string busqueda = "")
        //{
        //    var viaje = await _context.Viaje.FindAsync(viajeId);
        //    if (viaje == null) return NotFound();

        //    // Coordinadores ya asignados
        //    var asignadosIds = _context.ViajeCoordinador
        //        .Where(vc => vc.ViajeId == viajeId)
        //        .Select(vc => vc.CoordinadorId)
        //        .ToList();

        //    // Coordinadores disponibles para agregar
        //    var disponibles = await _context.Coordinador
        //        .Where(c => !asignadosIds.Contains(c.Id) &&
        //                   (c.Nombre.Contains(busqueda) || c.Apellido.Contains(busqueda) || c.Pasaporte.Contains(busqueda)))
        //        .ToListAsync();

        //    ViewBag.ViajeId = viajeId;
        //    ViewBag.Busqueda = busqueda;

        //    return View(disponibles); // Retorna la vista AgregarCoordinador.cshtml
        //}


        [Authorize(Roles = "Admin,Administracion")]
        public async Task<IActionResult> AgregarCoordinador(
    int viajeId,
    string busquedaNombre = "",
    string busquedaApellido = "",
    string busquedaDni = "",
    int pagina = 1,
    int registrosPorPagina = 10,
    bool mostrarTodos = false)
        {
            var viaje = await _context.Viaje.FindAsync(viajeId);
            if (viaje == null) return NotFound();

            // IDs ya asignados
            var coordinadoresAsignadosIds = _context.ViajeCoordinador
                .Where(vc => vc.ViajeId == viajeId)
                .Select(vc => vc.CoordinadorId)
                .ToList();

            // Base query
            var query = _context.Coordinador
                .Where(c => !coordinadoresAsignadosIds.Contains(c.Id));

            // Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(busquedaNombre))
                query = query.Where(c => c.Nombre.Contains(busquedaNombre));

            if (!string.IsNullOrWhiteSpace(busquedaApellido))
                query = query.Where(c => c.Apellido.Contains(busquedaApellido));

            if (!string.IsNullOrWhiteSpace(busquedaDni))
                query = query.Where(c => c.Pasaporte.Contains(busquedaDni));

            int totalRegistros = await query.CountAsync();

            // Paginación solo si no se muestra todo
            List<Coordinador> coordinadores;
            if (mostrarTodos)
            {
                coordinadores = await query.OrderBy(c => c.Apellido).ToListAsync();
            }
            else
            {
                coordinadores = await query
                    .OrderBy(c => c.Apellido)
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToListAsync();
            }

            // Armar ViewModel
            var vm = new CoordinadorVM
            {
                coordinador = coordinadores,
                busquedaNombre = busquedaNombre,
                busquedaApellido = busquedaApellido,
                busquedaDni = busquedaDni,
                MostrarTodos = mostrarTodos,
                paginador = new Paginador
                {
                    PaginaActual = pagina,
                    TotalRegistros = totalRegistros,
                    RegistrosPorPagina = registrosPorPagina,
                    ValoresQueryString = new Dictionary<string, string>
            {
                {"viajeId", viajeId.ToString()},
                {"busquedaNombre", busquedaNombre},
                {"busquedaApellido", busquedaApellido},
                {"busquedaDni", busquedaDni},
                {"mostrarTodos", mostrarTodos.ToString()}
            }
                }
            };

            ViewBag.ViajeId = viajeId;

            return View(vm);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPasajeroDelViaje(int viajeId, int pasajeroId)
        {
            var viajePasajero = await _context.ViajePasajero
                .FirstOrDefaultAsync(vp => vp.ViajeId == viajeId && vp.PasajeroId == pasajeroId);

            if (viajePasajero != null)
            {
                _context.ViajePasajero.Remove(viajePasajero);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VerPasajeros", new { id = viajeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCoordinadorDelViaje(int viajeId, int coordinadorId)
        {
            var viajeCoordinador = await _context.ViajeCoordinador
                .FirstOrDefaultAsync(vc => vc.ViajeId == viajeId && vc.CoordinadorId == coordinadorId);

            if (viajeCoordinador != null)
            {
                _context.ViajeCoordinador.Remove(viajeCoordinador);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VerCoordinador", new { id = viajeId });
        }





    }




}

