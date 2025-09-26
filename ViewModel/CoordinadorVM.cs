using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class CoordinadorVM
    {
        public List<Coordinador> coordinador { get; set; }
        public string busqNombre { get; set; }
        public string busqApellido { get; set; }
        public string busqPasaporte { get; set; }
        public Paginador paginador { get; set; }
    }
}
