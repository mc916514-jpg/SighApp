using System.Collections.Generic;
using System.Threading.Tasks;
using SighApp.Models;

namespace SighApp.Services
{
    public interface IDashboardService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
    }

    public class DashboardStats
    {
        public int TotalPacientes { get; set; }
        public int TotalMedicos { get; set; }
        public int TotalCitasHoy { get; set; }
        public int TotalCitasPendientes { get; set; }
        
        public IEnumerable<Cita> CitasRecientes { get; set; } = new List<Cita>();
        public Dictionary<string, int> CitasPorEspecialidad { get; set; } = new Dictionary<string, int>();
    }
}
