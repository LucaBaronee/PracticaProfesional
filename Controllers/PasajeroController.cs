using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using ProyetoSetilPF.Data;
using ProyetoSetilPF.Models;
using ProyetoSetilPF.ViewModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Globalization;
using System.Text;
using ProyetoSetilPF.Data.Migrations;

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

        //[Authorize(Roles = "Admin,Administracion")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,FechaNacimiento,Telefono,Vencimiento")] Pasajero pasajero)
        //{
        //    if (id != pasajero.Id)
        //        return NotFound();

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var pasajeroDb = await _context.Pasajero.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        //            if (pasajeroDb == null) return NotFound();

        //            // Mantener la foto anterior si no se subió ninguna
        //            var nuevaFoto = HttpContext.Request.Form.Files.FirstOrDefault();
        //            if (nuevaFoto != null && nuevaFoto.Length > 0)
        //            {
        //                pasajero.FotoPasaporte = cargarFoto(pasajeroDb.FotoPasaporte);
        //            }
        //            else
        //            {
        //                pasajero.FotoPasaporte = pasajeroDb.FotoPasaporte;
        //            }

        //            // Mantener la bandera EnViaje y Activo
        //            pasajero.EnViaje = pasajeroDb.EnViaje;
        //            pasajero.Activo = pasajeroDb.Activo;

        //            _context.Update(pasajero);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!_context.Pasajero.Any(e => e.Id == pasajero.Id))
        //                return NotFound();
        //            else
        //                throw;
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
        //    return View(pasajero);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Apellido,Nombre,Edad,SexoId,Pasaporte,FotoPasaporte,FechaNacimiento,Telefono,Vencimiento")] Pasajero pasajero)
        //{
        //    if (id != pasajero.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            // Obtener la entidad original de la base para mantener propiedades que no se editan
        //            var coordinadorDb = await _context.Coordinador.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        //            if (coordinadorDb == null) return NotFound();

        //            // Mantener la foto anterior si no se subió ninguna nueva
        //            var nuevaFoto = HttpContext.Request.Form.Files.FirstOrDefault();
        //            if (nuevaFoto != null && nuevaFoto.Length > 0)
        //            {
        //                pasajero.FotoPasaporte = cargarFoto(coordinadorDb.FotoPasaporte);
        //            }
        //            else
        //            {
        //                pasajero.FotoPasaporte = coordinadorDb.FotoPasaporte;
        //            }

        //            // Mantener las banderas o propiedades que no se modifican desde el formulario
        //            pasajero.Activo = coordinadorDb.Activo;
        //            pasajero.EnViaje = coordinadorDb.EnViaje; // si aplica

        //            _context.Update(pasajero);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!PasajeroExists(pasajero.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["SexoId"] = new SelectList(_context.Sexo, "Id", "Descripcion", pasajero.SexoId);
        //    return View(pasajero);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
    [Bind("Id,Apellido,Nombre,SexoId,Pasaporte,FechaNacimiento,Telefono,Vencimiento,FotoPasaporte")] Pasajero pasajero)
        {
            if (id != pasajero.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener datos anteriores para no perder nada
                    var pasajeroDb = await _context.Pasajero
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (pasajeroDb == null)
                        return NotFound();

                    // FOTO PASAPORTE
                    var nuevaFoto = HttpContext.Request.Form.Files.FirstOrDefault();

                    if (nuevaFoto != null && nuevaFoto.Length > 0)
                    {
                        pasajero.FotoPasaporte = cargarFoto(pasajeroDb.FotoPasaporte);
                    }
                    else
                    {
                        pasajero.FotoPasaporte = pasajeroDb.FotoPasaporte;
                    }

                    // Mantener propiedades no editables
                    pasajero.Activo = pasajeroDb.Activo;
                    pasajero.EnViaje = pasajeroDb.EnViaje;

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




        public IActionResult Importar()
        {
            return View();
        }


        //[HttpPost]
        //public async Task<IActionResult> ImportarPasajeros(IFormFile archivoExcel, IFormFile archivoZip)
        //{
        //    if (archivoExcel == null || archivoExcel.Length == 0)
        //    {
        //        TempData["Error"] = "Debe seleccionar un archivo Excel.";
        //        return RedirectToAction("Index");
        //    }

        //    if (archivoZip == null || archivoZip.Length == 0)
        //    {
        //        TempData["Error"] = "Debe seleccionar un archivo ZIP con las fotos.";
        //        return RedirectToAction("Index");
        //    }

        //    // Ruta fotos
        //    string rutaFotos = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
        //    if (!Directory.Exists(rutaFotos))
        //        Directory.CreateDirectory(rutaFotos);

        //    // Leer ZIP y guardar archivos en memoria
        //    Dictionary<string, byte[]> fotosZip = new();
        //    using (var zipStream = archivoZip.OpenReadStream())
        //    using (var zip = new System.IO.Compression.ZipArchive(zipStream))
        //    {
        //        foreach (var entry in zip.Entries)
        //        {
        //            // Solo archivos reales (ignora carpetas)
        //            if (!string.IsNullOrEmpty(entry.Name) && entry.Length > 0)
        //            {
        //                using var streamEntry = entry.Open();
        //                using var ms = new MemoryStream();
        //                streamEntry.CopyTo(ms);

        //                fotosZip[entry.Name.ToLower()] = ms.ToArray();
        //            }
        //        }
        //    }

        //    // Función para normalizar nombres (quitar espacios, guiones, etc.)
        //    string Normalizar(string texto) => new string(texto.Where(char.IsLetterOrDigit).ToArray()).ToLower();

        //    // Leer Excel
        //    using var stream = new MemoryStream();
        //    await archivoExcel.CopyToAsync(stream);
        //    using var package = new OfficeOpenXml.ExcelPackage(stream);

        //    var hoja = package.Workbook.Worksheets[0];
        //    int filas = hoja.Dimension.Rows;

        //    for (int i = 2; i <= filas; i++)
        //    {
        //        string apellido = hoja.Cells[i, 1].Text.Trim();
        //        string nombre = hoja.Cells[i, 2].Text.Trim();
        //        string sexoTexto = hoja.Cells[i, 3].Text.Trim().ToLower();
        //        string pasaporte = hoja.Cells[i, 4].Text.Trim();
        //        string fechaNacimientoTxt = hoja.Cells[i, 5].Text.Trim();
        //        string telefono = hoja.Cells[i, 6].Text.Trim();
        //        string vencimientoTxt = hoja.Cells[i, 7].Text.Trim();

        //        if (_context.Pasajero.Any(p => p.Pasaporte == pasaporte))
        //            continue;

        //        // Sexo
        //        var sexo = _context.Sexo.FirstOrDefault(s => s.Descripcion.ToLower() == sexoTexto);
        //        if (sexo == null)
        //        {
        //            sexo = new Sexo { Descripcion = char.ToUpper(sexoTexto[0]) + sexoTexto.Substring(1) };
        //            _context.Sexo.Add(sexo);
        //            await _context.SaveChangesAsync();
        //        }

        //        var pasajero = new Pasajero
        //        {
        //            Apellido = apellido,
        //            Nombre = nombre,
        //            SexoId = sexo.Id,
        //            Pasaporte = pasaporte,
        //            Telefono = telefono,
        //            FechaNacimiento = DateTime.Parse(fechaNacimientoTxt),
        //            Vencimiento = DateTime.Parse(vencimientoTxt),
        //        };

        //        // Foto: buscar por nombre normalizado
        //        string nombreEsperado = Normalizar($"{nombre}{apellido}{pasaporte}");
        //        var foto = fotosZip.FirstOrDefault(f => Normalizar(f.Key).Contains(nombreEsperado));

        //        if (foto.Value != null)
        //        {
        //            string extension = System.IO.Path.GetExtension(foto.Key);
        //            string nombreFinal = $"{nombre}_{apellido}_{pasaporte}{extension}".ToLower();

        //            string rutaFinal = Path.Combine(rutaFotos, nombreFinal);
        //            System.IO.File.WriteAllBytes(rutaFinal, foto.Value);

        //            pasajero.FotoPasaporte = nombreFinal;
        //        }

        //        _context.Pasajero.Add(pasajero);
        //        await _context.SaveChangesAsync();
        //    }

        //    TempData["Mensaje"] = "Importación completada con éxito.";
        //    return RedirectToAction("Index");
        //}


        [HttpPost]
        public async Task<IActionResult> ImportarPasajeros(IFormFile archivoExcel, IFormFile archivoZip)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo Excel.";
                return RedirectToAction("Index");
            }

            if (archivoZip == null || archivoZip.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo ZIP con las fotos.";
                return RedirectToAction("Index");
            }

            // Ruta fotos
            string rutaFotos = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
            if (!Directory.Exists(rutaFotos))
                Directory.CreateDirectory(rutaFotos);

            // Leer ZIP y guardar archivos en memoria
            Dictionary<string, byte[]> fotosZip = new();
            using (var zipStream = archivoZip.OpenReadStream())
            using (var zip = new System.IO.Compression.ZipArchive(zipStream))
            {
                foreach (var entry in zip.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Name) && entry.Length > 0)
                    {
                        using var streamEntry = entry.Open();
                        using var ms = new MemoryStream();
                        streamEntry.CopyTo(ms);
                        fotosZip[entry.Name.ToLower()] = ms.ToArray();
                    }
                }
            }

            // Función para normalizar nombres y pasaportes
            string Normalizar(string texto)
            {
                if (string.IsNullOrEmpty(texto)) return "";
                texto = texto.ToLowerInvariant();

                // Quitar acentos
                var normalizedString = texto.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();
                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                        stringBuilder.Append(c);
                }
                texto = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

                // Quitar espacios y caracteres no alfanuméricos
                texto = new string(texto.Where(char.IsLetterOrDigit).ToArray());
                return texto;
            }

            // Leer Excel
            using var stream = new MemoryStream();
            await archivoExcel.CopyToAsync(stream);
            using var package = new OfficeOpenXml.ExcelPackage(stream);

            var hoja = package.Workbook.Worksheets[0];
            int filas = hoja.Dimension.Rows;

            for (int i = 2; i <= filas; i++)
            {
                string apellido = hoja.Cells[i, 1].Text.Trim();
                string nombre = hoja.Cells[i, 2].Text.Trim();
                string sexoTexto = hoja.Cells[i, 3].Text.Trim().ToLower();
                string pasaporte = hoja.Cells[i, 4].Text.Trim();
                string fechaNacimientoTxt = hoja.Cells[i, 5].Text.Trim();
                string telefono = hoja.Cells[i, 6].Text.Trim();
                string vencimientoTxt = hoja.Cells[i, 7].Text.Trim();

                // Normalizar clave para comparar
                string clave = Normalizar($"{nombre}{apellido}{pasaporte}");

                //// Buscar pasajero activo existente
                //var pasajeroExistente = await _context.Pasajero
                //    .Where(p => p.Activo)
                //    .FirstOrDefaultAsync(p => Normalizar(p.Nombre + p.Apellido + p.Pasaporte) == clave);


                // Traer solo los pasajeros activos a memoria
                var pasajerosActivos = await _context.Pasajero
                    .Where(p => p.Activo)
                    .ToListAsync();

                // Filtrar usando Normalizar en memoria
                var pasajeroExistente = pasajerosActivos
                    .FirstOrDefault(p => Normalizar(p.Nombre + p.Apellido + p.Pasaporte) == clave);

                // Sexo
                var sexo = _context.Sexo.FirstOrDefault(s => s.Descripcion.ToLower() == sexoTexto);
                if (sexo == null)
                {
                    sexo = new Sexo { Descripcion = char.ToUpper(sexoTexto[0]) + sexoTexto.Substring(1) };
                    _context.Sexo.Add(sexo);
                    await _context.SaveChangesAsync();
                }

                if (pasajeroExistente != null)
                {
                    // Actualizar datos existentes
                    pasajeroExistente.Nombre = nombre;
                    pasajeroExistente.Apellido = apellido;
                    pasajeroExistente.SexoId = sexo.Id;
                    pasajeroExistente.Telefono = telefono;
                    pasajeroExistente.FechaNacimiento = DateTime.Parse(fechaNacimientoTxt);
                    pasajeroExistente.Vencimiento = DateTime.Parse(vencimientoTxt);

                    // Actualizar foto si existe
                    var foto = fotosZip.FirstOrDefault(f => Normalizar(f.Key).Contains(clave));
                    if (foto.Value != null)
                    {
                        string extension = Path.GetExtension(foto.Key);
                        string nombreFinal = $"{nombre}_{apellido}_{pasaporte}{extension}".ToLower();
                        string rutaFinal = Path.Combine(rutaFotos, nombreFinal);
                        System.IO.File.WriteAllBytes(rutaFinal, foto.Value);
                        pasajeroExistente.FotoPasaporte = nombreFinal;
                    }

                    _context.Pasajero.Update(pasajeroExistente);
                }
                else
                {
                    // Crear nuevo pasajero
                    var pasajero = new Pasajero
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        SexoId = sexo.Id,
                        Pasaporte = pasaporte,
                        Telefono = telefono,
                        FechaNacimiento = DateTime.Parse(fechaNacimientoTxt),
                        Vencimiento = DateTime.Parse(vencimientoTxt)
                    };

                    // Foto
                    var foto = fotosZip.FirstOrDefault(f => Normalizar(f.Key).Contains(clave));
                    if (foto.Value != null)
                    {
                        string extension = Path.GetExtension(foto.Key);
                        string nombreFinal = $"{nombre}_{apellido}_{pasaporte}{extension}".ToLower();
                        string rutaFinal = Path.Combine(rutaFotos, nombreFinal);
                        System.IO.File.WriteAllBytes(rutaFinal, foto.Value);
                        pasajero.FotoPasaporte = nombreFinal;
                    }

                    _context.Pasajero.Add(pasajero);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = "Importación completada con éxito.";
            return RedirectToAction("Index");
        }



    }
}
