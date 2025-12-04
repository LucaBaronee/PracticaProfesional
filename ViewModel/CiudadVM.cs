using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class CiudadVM
    {
        public List<Ciudad> ciudad { get; set; }
        public string busquedaNombre { get; set; }
        public Paginador paginador { get; set; }
    }
}
