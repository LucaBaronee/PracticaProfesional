using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyetoSetilPF.Models
{
    public class Pasajero
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio")]
        public int SexoId { get; set; }
        public Sexo? Sexo { get; set; }

        [Required(ErrorMessage = "El pasaporte es obligatorio")]
        [StringLength(20, ErrorMessage = "El pasaporte no puede superar los 20 caracteres")]
        public string Pasaporte { get; set; }

        public string? FotoPasaporte { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Pasajero), nameof(ValidarFechaNacimiento))]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime Vencimiento { get; set; }

        public List<ViajePasajero>? ViajePasajero { get; set; }

        public bool Activo { get; set; } = true;
        public bool EnViaje { get; set; } = false;

        [NotMapped]
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        // Validación personalizada para que la fecha de nacimiento no sea futura
        public static ValidationResult? ValidarFechaNacimiento(DateTime fechaNacimiento, ValidationContext context)
        {
            if (fechaNacimiento > DateTime.Today)
                return new ValidationResult("La fecha de nacimiento no puede ser mayor al día de hoy");
            return ValidationResult.Success;
        }
    }
}
