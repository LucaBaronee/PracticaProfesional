namespace ProyetoSetilPF.Models
{
    public class TipoMovimiento
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } 

       
        public List<MovimientoViaje>? Movimientos { get; set; }
    }
}
