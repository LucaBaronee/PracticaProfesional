namespace ProyetoSetilPF.Models
{
    public class ViajeCiudad
    {
        public int CiudadId { get; set; }
        public Ciudad? Ciudad { get; set; }
        public int ViajeId { get; set; }
        public Viaje? Viaje { get; set; }
    }
}
