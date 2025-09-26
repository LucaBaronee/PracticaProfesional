﻿using ProyetoSetilPF.Models;

namespace ProyetoSetilPF.ViewModel
{
    public class PasajeroVm
    {
        public List<Pasajero> pasajero { get; set; }
        public string busquedaNombre { get; set; }
        public string busquedaApellido { get; set; }
        public string busquedaDni { get; set; }
        public Paginador paginador { get; set; }
        public bool MostrarTodos { get; set; } = false;
    }
}
