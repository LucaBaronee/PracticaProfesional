namespace ProyetoSetilPF.Models
{
    public class ViajeCoordinador
    {
        public int CoordinadorId { get; set; }

        public Coordinador? Coordinador { get; set; }
        public int ViajeId { get; set; }
        public Viaje? Viaje { get; set; }
    }
}
