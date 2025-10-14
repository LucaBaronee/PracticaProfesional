namespace ProyetoSetilPF.Models
{
    public class ViajePasajero
    {
        public int PasajeroId { get; set; }
        public Pasajero? Pasajero { get; set; }
        public int ViajeId { get; set; }
        public Viaje? Viaje { get; set; }
        public int AgenciaId { get; set; }          // 👈 Agencia para este viaje
        public Agencia Agencia { get; set; }
    }
}
