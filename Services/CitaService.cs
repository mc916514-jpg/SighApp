using System;
using System.Linq;
using System.Threading.Tasks;
using SighApp.Repository;

namespace SighApp.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IMedicoRepository _medicoRepository;

        public CitaService(ICitaRepository citaRepository, IMedicoRepository medicoRepository)
        {
            _citaRepository = citaRepository;
            _medicoRepository = medicoRepository;
        }

        public async Task<(bool IsAvailable, string Message)> ValidarDisponibilidadAsync(int medicoId, DateTime fechaHora, int? citaIdExcluir = null)
        {
            // 1. Validar que el médico exista y esté activo
            var medico = await _medicoRepository.GetByIdAsync(medicoId);
            if (medico == null)
            {
                return (false, "El médico seleccionado no existe en el sistema.");
            }
            if (!medico.Activo)
            {
                return (false, $"El {medico.NombreCompleto} se encuentra inactivo y no puede recibir citas.");
            }

            // 2. Validar que la fecha y hora no sean del pasado
            if (fechaHora < DateTime.Now)
            {
                return (false, "No se pueden programar citas para una fecha y hora del pasado.");
            }

            // 3. Validar horario de atención: Lunes a Viernes entre 8:00 AM y 8:00 PM (20:00)
            int hora = fechaHora.Hour;
            DayOfWeek dia = fechaHora.DayOfWeek;

            if (dia == DayOfWeek.Saturday || dia == DayOfWeek.Sunday)
            {
                return (false, "La clínica solo atiende citas de Lunes a Viernes.");
            }

            if (hora < 8 || hora >= 20)
            {
                return (false, "El horario de atención es de 08:00 AM a 08:00 PM.");
            }

            // 4. Obtener citas del médico para ese día
            var citasDia = await _citaRepository.GetByMedicoAndFechaAsync(medicoId, fechaHora);

            // 5. Validar traslape (intervalo de 30 minutos)
            foreach (var cita in citasDia)
            {
                // Si es la cita que se está editando, la excluimos de la validación
                if (citaIdExcluir.HasValue && cita.Id == citaIdExcluir.Value)
                {
                    continue;
                }

                double diferenciaMinutos = Math.Abs((cita.FechaHora - fechaHora).TotalMinutes);
                if (diferenciaMinutos < 30)
                {
                    string horaExistente = cita.FechaHora.ToString("hh:mm tt");
                    return (false, $"Traslape de horario detectado. El médico ya tiene una cita agendada para las {horaExistente} (Intervalo requerido: 30 minutos).");
                }
            }

            return (true, "El horario se encuentra disponible.");
        }
    }
}
