namespace ProyetoSetilPF.Models
{
    public class ViajePasajero
    {
        public int PasajeroId { get; set; }
        public Pasajero? Pasajero { get; set; }
        public int ViajeId { get; set; }
        public Viaje? Viaje { get; set; }
    }
}
