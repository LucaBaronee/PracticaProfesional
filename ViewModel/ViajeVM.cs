using ProyetoSetilPF.Models;
namespace ProyetoSetilPF.ViewModel;

public class ViajeVM
{
    public List<Viaje> viaje { get; set; }
    public string busquedaNombre { get; set; }
    public Paginador paginador { get; set; }
}
