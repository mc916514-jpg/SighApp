using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface ITratamientoRepository
    {
        Task<IEnumerable<Tratamiento>> GetByDiagnosticoIdAsync(int diagnosticoId);
        Task<Tratamiento?> GetByIdAsync(int id);
        Task<int> AddAsync(Tratamiento tratamiento);
        Task<bool> UpdateAsync(Tratamiento tratamiento);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllByDiagnosticoIdAsync(int diagnosticoId);
    }
}
