using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class CiudadVM
    {
        public List<Ciudad> ciudad { get; set; }
        public string busqNombre { get; set; }
        public Paginador paginador { get; set; }
    }
}
