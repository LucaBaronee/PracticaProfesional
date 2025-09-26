using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ViajeCiudad>()
              .HasKey(vc => new { vc.ViajeId, vc.CiudadId });  // Llave compuesta

            modelBuilder.Entity<ViajeCiudad>()
                .HasOne(vc => vc.Viaje)                         // Relación con Viaje
                .WithMany(v => v.ViajeCiudad)                   // 'ViajeCiudad' debe ser la lista en 'Viaje'
                .HasForeignKey(vc => vc.ViajeId);

            modelBuilder.Entity<ViajeCiudad>()
                .HasOne(vc => vc.Ciudad)                        // Relación con Ciudad
                .WithMany(c => c.ViajeCiudad)                   // 'ViajeCiudad' debe ser la lista en 'Ciudad'
                .HasForeignKey(vc => vc.CiudadId);

            modelBuilder.Entity<ViajeCoordinador>()
              .HasKey(vc => new { vc.ViajeId, vc.CoordinadorId });

            modelBuilder.Entity<ViajeCoordinador>()
                .HasOne(vc => vc.Viaje)
                .WithMany(v => v.ViajeCoordinador)
                .HasForeignKey(vc => vc.ViajeId);

            modelBuilder.Entity<ViajeCoordinador>()
                .HasOne(vc => vc.Coordinador)
                .WithMany(c => c.ViajeCoordinador)
                .HasForeignKey(vc => vc.CoordinadorId);


            modelBuilder.Entity<ViajePasajero>()
             .HasKey(vc => new { vc.ViajeId, vc.PasajeroId });

            modelBuilder.Entity<ViajePasajero>()
                .HasOne(vc => vc.Viaje)
                .WithMany(v => v.ViajePasajero)
                .HasForeignKey(vc => vc.ViajeId);

            modelBuilder.Entity<ViajePasajero>()
                .HasOne(vc => vc.Pasajero)
                .WithMany(c => c.ViajePasajero)
                .HasForeignKey(vc => vc.PasajeroId);



            // Relación Viaje - Movimiento (1:N)
            modelBuilder.Entity<MovimientoViaje>()
                .HasOne(m => m.Viaje)
                .WithMany(v => v.MovimientosViaje)
                .HasForeignKey(m => m.ViajeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Movimiento - TipoMovimiento (1:N)
            modelBuilder.Entity<MovimientoViaje>()
                .HasOne(m => m.TipoMovimiento)
                .WithMany(t => t.Movimientos)
                .HasForeignKey(m => m.TipoMovimientoId);
            modelBuilder.Entity<DocumentoViaje>()
            .HasOne(d => d.Viaje)
            .WithMany(v => v.DocumentosViaje)
            .HasForeignKey(d => d.ViajeId);
        }


        public DbSet<DocumentoViaje> DocumentosViaje { get; set; }

        public DbSet<Ciudad> Ciudad { get; set; }
        public DbSet<Pasajero> Pasajero { get; set; }
        public DbSet<Coordinador> Coordinador { get; set; }
        public DbSet<Viaje> Viaje { get; set; }
        public DbSet<Sexo> Sexo { get; set; }
        public DbSet<ViajeCiudad> ViajeCiudad { get; set; }
        public DbSet<ViajeCoordinador> ViajeCoordinador { get; set; }
        public DbSet<ViajePasajero> ViajePasajero { get; set; }

        // Movimientos
        public DbSet<MovimientoViaje> MovimientosViaje { get; set; }
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; }

    }
}
