using System.ComponentModel.DataAnnotations;

namespace ProyetoSetilPF.Models
{
    public class Viaje
    {

        public int Id { get; set; }
        public string Descripcion { get; set; }


        [Required]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "La fecha de ida debe ser mayor a hoy.")]

        public DateTime FechaIda { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [GreaterThan("FechaIda", ErrorMessage = "La fecha de vuelta no puede ser menor que la fecha de ida.")]
        public DateTime FechaVuelta { get; set; }

        public decimal Balance { get; set; }
        public int MonedaId { get; set; }
        public Moneda? Moneda { get; set; }
        public bool Activo { get; set; } = true;

        public List<MovimientoViaje>? MovimientosViaje { get; set; }
        public List<DocumentoViaje>? DocumentosViaje { get; set; } 
        public List<ViajeCiudad>? ViajeCiudad { get; set; }
        public List<ViajePasajero>? ViajePasajero { get; set; }
        public List<ViajeCoordinador>? ViajeCoordinador { get; set; }
    }
    // Validación de fechas futuras
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime fecha)
            {
                return fecha.Date > DateTime.Now.Date;
            }
            return false;
        }
    }

    // Validación de fecha mayor a otra propiedad
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public GreaterThanAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherProperty == null)
                return new ValidationResult($"Propiedad desconocida: {_otherPropertyName}");

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (value is DateTime thisDate && otherValue is DateTime otherDate)
            {
                if (thisDate >= otherDate)
                    return ValidationResult.Success;
                else
                    return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
