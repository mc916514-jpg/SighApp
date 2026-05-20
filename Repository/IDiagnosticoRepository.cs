using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface IDiagnosticoRepository
    {
        Task<IEnumerable<Diagnostico>> GetHistorialClinicoAsync(int pacienteId);
        Task<Diagnostico?> GetByIdAsync(int id);
        Task<Diagnostico?> GetByCitaIdAsync(int citaId);
        Task<int> AddAsync(Diagnostico diagnostico);
        Task<bool> UpdateAsync(Diagnostico diagnostico);
        Task<bool> DeleteAsync(int id);
    }
}
