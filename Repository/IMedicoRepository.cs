using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface IMedicoRepository
    {
        Task<IEnumerable<Medico>> GetAllAsync(int? especialidadId = null, string? search = null);
        Task<Medico?> GetByIdAsync(int id);
        Task<int> AddAsync(Medico medico);
        Task<bool> UpdateAsync(Medico medico);
        Task<bool> DeleteAsync(int id);
    }
}
