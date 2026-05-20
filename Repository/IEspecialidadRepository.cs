using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface IEspecialidadRepository
    {
        Task<IEnumerable<Especialidad>> GetAllAsync();
        Task<Especialidad?> GetByIdAsync(int id);
        Task<int> AddAsync(Especialidad especialidad);
        Task<bool> UpdateAsync(Especialidad especialidad);
        Task<bool> DeleteAsync(int id);
    }
}
