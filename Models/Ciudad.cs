namespace ProyetoSetilPF.Models
{
    public class Ciudad
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public int CodigoPostal { get; set; }
        public List<ViajeCiudad>? ViajeCiudad { get; set; }
    }
}
