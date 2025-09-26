namespace ProyetoSetilPF.Models
{
    public class Viaje
    {

        public int Id { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaIda { get; set; }
        public DateTime FechaVuelta { get; set; }
        public decimal Balance { get; set; }
        public bool Activo { get; set; } = true;

        public List<MovimientoViaje>? MovimientosViaje { get; set; }
        public List<DocumentoViaje>? DocumentosViaje { get; set; } 
        public List<ViajeCiudad>? ViajeCiudad { get; set; }
        public List<ViajePasajero>? ViajePasajero { get; set; }
        public List<ViajeCoordinador>? ViajeCoordinador { get; set; }
    }
}
