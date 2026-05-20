using System.ComponentModel.DataAnnotations;

namespace SighApp.Models
{
    public class Tratamiento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El diagnóstico asociado es obligatorio.")]
        [Display(Name = "Diagnóstico")]
        public int DiagnosticoId { get; set; }

        [Required(ErrorMessage = "El nombre del medicamento es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del medicamento no puede superar los 200 caracteres.")]
        [Display(Name = "Medicamento")]
        public string Medicamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dosis es obligatoria (ej: 500mg, 1 tableta, 5ml).")]
        [StringLength(100, ErrorMessage = "La dosis no puede superar los 100 caracteres.")]
        [Display(Name = "Dosis")]
        public string Dosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "La frecuencia es obligatoria (ej: Cada 8 horas, Una vez al día).")]
        [StringLength(100, ErrorMessage = "La frecuencia no puede superar los 100 caracteres.")]
        [Display(Name = "Frecuencia")]
        public string Frecuencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria (ej: 7 días, 1 mes).")]
        [StringLength(100, ErrorMessage = "La duración no puede superar los 100 caracteres.")]
        [Display(Name = "Duración")]
        public string Duracion { get; set; } = string.Empty;
    }
}
