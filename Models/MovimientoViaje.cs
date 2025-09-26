namespace ProyetoSetilPF.Models
{
    public class MovimientoViaje
    {
        public int Id { get; set; }

        public int? ViajeId { get; set; }
        public Viaje? Viaje { get; set; }

        public int TipoMovimientoId { get; set; }
        public TipoMovimiento? TipoMovimiento { get; set; }

        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
    }
}
