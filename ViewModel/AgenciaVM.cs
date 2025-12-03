using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class AgenciaVM
    {
        public List<Agencia> agencia { get; set; }
        public string busquedaNombre { get; set; }
        public Paginador paginador { get; set; }

    }
}
