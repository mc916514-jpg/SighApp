using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Repository
{
    public interface IPacienteRepository
    {
        Task<IEnumerable<Paciente>> GetAllAsync(string? search = null);
        Task<Paciente?> GetByIdAsync(int id);
        Task<Paciente?> GetByCedulaAsync(string cedula);
        Task<int> AddAsync(Paciente paciente);
        Task<bool> UpdateAsync(Paciente paciente);
        Task<bool> DeleteAsync(int id);
    }
}
