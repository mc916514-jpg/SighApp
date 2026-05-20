using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Services;

namespace SighApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IExternalApiService _externalApiService;

        public HomeController(IDashboardService dashboardService, IExternalApiService externalApiService)
        {
            _dashboardService = dashboardService;
            _externalApiService = externalApiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                var weather = await _externalApiService.GetCurrentWeatherAsync();
                var healthTip = await _externalApiService.GetDailyHealthTipAsync();

                ViewBag.Weather = weather;
                ViewBag.HealthTip = healthTip;

                return View(stats);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar las estadísticas del Dashboard: " + ex.Message;
                return View(new DashboardStats());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
