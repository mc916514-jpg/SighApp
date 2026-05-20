using System;
using System.Threading.Tasks;

namespace SighApp.Services
{
    public interface ICitaService
    {
        Task<(bool IsAvailable, string Message)> ValidarDisponibilidadAsync(int medicoId, DateTime fechaHora, int? citaIdExcluir = null);
    }
}
