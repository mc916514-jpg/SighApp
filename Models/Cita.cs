using System;
using System.ComponentModel.DataAnnotations;

namespace SighApp.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un paciente.")]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un médico.")]
        [Display(Name = "Médico")]
        public int MedicoId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de la cita es obligatoria.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "El motivo de la consulta es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        [Display(Name = "Motivo de la Cita")]
        public string Motivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El estado de la cita es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Realizada, Cancelada

        // Propiedades de ayuda para visualización (se mapean manualmente con ADO.NET)
        [Display(Name = "Paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Display(Name = "Médico")]
        public string NombreMedico { get; set; } = string.Empty;

        [Display(Name = "Especialidad")]
        public string NombreEspecialidad { get; set; } = string.Empty;

        [Display(Name = "Cédula del Paciente")]
        public string CedulaPaciente { get; set; } = string.Empty;
    }
}
