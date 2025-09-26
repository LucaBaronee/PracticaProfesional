﻿using Microsoft.AspNetCore.Identity;

namespace ProyetoSetilPF.Models
{
    public class Coordinador
    {
        public int Id { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public int SexoId { get; set; }
        public Sexo? Sexo { get; set; }
        public string Pasaporte { get; set; }
        public string? FotoPasaporte { get; set; }
        public DateTime Vencimiento { get; set; }

        public DateTime FechaNacimiento { get; set; }
        public int Telefono { get; set; }
        public List<ViajeCoordinador>? ViajeCoordinador { get; set; }
        public bool Activo { get; set; } = true;

        public string Email { get; set; }

        // 🔑 Relación con IdentityUser
        public string? UserId { get; set; }   // ID del usuario en AspNetUsers
        public IdentityUser? User { get; set; } // Navegación al usuario (para acceder al Email, UserName, etc.)
    }
}
