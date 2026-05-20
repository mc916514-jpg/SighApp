using System.Threading.Tasks;

namespace SighApp.Services
{
    public interface IExternalApiService
    {
        Task<WeatherData?> GetCurrentWeatherAsync();
        Task<string> GetDailyHealthTipAsync();
    }

    public class WeatherData
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public int WeatherCode { get; set; }
        public string Condition { get; set; } = "Despejado";
        public string IconClass { get; set; } = "bi-sun-fill";
    }
}
