namespace ProyetoSetilPF.Models
{
    public class Moneda
    {
        public int Id { get; set; }
        public string Nombre { get; set; }  // Ej: "Dólar estadounidense"
        public string Simbolo { get; set; }  // Ej: "$", "€"
        public string CodigoIso { get; set;  }  // Ej: "USD", "EUR", "ARS"

        // Relación con los viajes
        public List<Viaje>? Viaje { get; set; }
    }
}
