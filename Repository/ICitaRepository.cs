using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface ICitaRepository
    {
        Task<IEnumerable<Cita>> GetAllAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int? medicoId = null, int? especialidadId = null, string? search = null);
        Task<Cita?> GetByIdAsync(int id);
        Task<IEnumerable<Cita>> GetByMedicoAndFechaAsync(int medicoId, DateTime fecha);
        Task<int> AddAsync(Cita cita);
        Task<bool> UpdateAsync(Cita cita);
        Task<bool> UpdateEstadoAsync(int id, string estado);
        Task<bool> DeleteAsync(int id);
    }
}
