using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SighApp.Models
{
    public class Diagnostico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La cita asociada es obligatoria.")]
        [Display(Name = "Cita")]
        public int CitaId { get; set; }

        [Required(ErrorMessage = "La descripción del diagnóstico es obligatoria.")]
        [Display(Name = "Descripción del Diagnóstico")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Fecha de Diagnóstico")]
        public DateTime FechaDiagnostico { get; set; } = DateTime.Now;

        // Propiedades de ayuda para visualización (Mapeo manual con ADO.NET)
        [Display(Name = "Paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Display(Name = "Médico")]
        public string NombreMedico { get; set; } = string.Empty;

        [Display(Name = "Fecha de la Cita")]
        public DateTime FechaCita { get; set; }

        [Display(Name = "Motivo de la Cita")]
        public string MotivoCita { get; set; } = string.Empty;

        // Colección de tratamientos recetados en este diagnóstico
        public List<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
    }
}
