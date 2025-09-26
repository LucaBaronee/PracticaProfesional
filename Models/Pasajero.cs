namespace ProyetoSetilPF.Models
{
    public class Pasajero
    {
        public int Id { get; set; }
        public string Agencia { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public int SexoId { get; set; }
        public Sexo? Sexo { get; set; }
        public string Pasaporte { get; set; }
        public string? FotoPasaporte { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string RegOpc {  get; set; }
        public string PuntoSubida { get; set; }
        public int Telefono { get; set; }
        public DateTime Vencimiento { get; set; }
        public List<ViajePasajero>? ViajePasajero { get; set; }
        public bool Activo { get; set; } = true;
    }
}
