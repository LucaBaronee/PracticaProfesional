using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class ViajeDetallesVM
    {
        public Viaje Viaje { get; set; }
        public List<Coordinador> Coordinadores { get; set; }
        public List<Ciudad> Ciudades { get; set; }
        public List<Pasajero> Pasajeros { get; set; }
        public List<MovimientoViaje> Movimientos { get; set; }
        public List<DocumentoViaje> Archivos { get; set; }
    }
}
