using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SighApp.Services
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;

        public ExternalApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherData?> GetCurrentWeatherAsync()
        {
            try
            {
                // Consumir API pública de Open-Meteo (Coordenadas de Madrid, España por defecto)
                string url = "https://api.open-meteo.com/v1/forecast?latitude=40.4168&longitude=-3.7038&current_weather=true";
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(content))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("current_weather", out JsonElement currentWeather))
                        {
                            double temp = currentWeather.GetProperty("temperature").GetDouble();
                            double wind = currentWeather.GetProperty("windspeed").GetDouble();
                            int code = currentWeather.GetProperty("weathercode").GetInt32();

                            var (condition, icon) = MapWeatherCode(code);

                            return new WeatherData
                            {
                                Temperature = temp,
                                WindSpeed = wind,
                                WeatherCode = code,
                                Condition = condition,
                                IconClass = icon
                            };
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback silencioso en caso de caída de internet o error de red
            }

            // Fallback con datos por defecto elegantes para no romper la UI
            return new WeatherData
            {
                Temperature = 22.4,
                WindSpeed = 8.5,
                WeatherCode = 0,
                Condition = "Despejado (Clínica)",
                IconClass = "bi-sun-fill"
            };
        }

        public Task<string> GetDailyHealthTipAsync()
        {
            string[] tips = new[]
            {
                "Mantenerse hidratado mejora el rendimiento mental y previene dolores de cabeza.",
                "El ejercicio de al menos 30 minutos al día fortalece el sistema cardiovascular.",
                "Realizar pausas activas cada 2 horas en la oficina protege la salud de su columna.",
                "Dormir entre 7 y 8 horas diarias ayuda a regenerar el sistema inmunitario y reduce el estrés.",
                "Reducir el consumo de sal en los alimentos previene la hipertensión arterial.",
                "La higiene de manos adecuada es la medida más económica y eficaz para prevenir infecciones respiratorias."
            };

            var random = new Random();
            int index = random.Next(tips.Length);
            return Task.FromResult(tips[index]);
        }

        private (string Condition, string Icon) MapWeatherCode(int code)
        {
            return code switch
            {
                0 => ("Despejado", "bi-sun-fill text-warning"),
                1 or 2 or 3 => ("Parcialmente Nublado", "bi-cloud-sun-fill text-info"),
                45 or 48 => ("Niebla / Neblina", "bi-cloud-fog2-fill text-secondary"),
                51 or 53 or 55 => ("Llovizna Ligera", "bi-cloud-drizzle-fill text-info"),
                61 or 63 or 65 => ("Lluvia Moderada", "bi-cloud-rain-fill text-primary"),
                71 or 73 or 75 => ("Nevada Ligera", "bi-cloud-snow-fill text-light"),
                80 or 81 or 82 => ("Lluvia Fuerte", "bi-cloud-rain-heavy-fill text-primary"),
                95 or 96 or 99 => ("Tormenta Eléctrica", "bi-cloud-lightning-rain-fill text-warning"),
                _ => ("Despejado", "bi-sun-fill text-warning")
            };
        }
    }
}
