namespace ProyetoSetilPF.Models
{
    public class DocumentoViaje
    {
        public int Id { get; set; }

        public string NombreArchivo { get; set; }   // ejemplo: comprobante.pdf
        public string RutaArchivo { get; set; }     // ejemplo: /uploads/viajes/5/comprobante.pdf

        // 🔹 Relación con Viaje
        public int ViajeId { get; set; }
        public Viaje? Viaje { get; set; }
    }
}
