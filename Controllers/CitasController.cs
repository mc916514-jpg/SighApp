using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Repository;
using SighApp.Services;

namespace SighApp.Controllers
{
    public class CitasController : Controller
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMedicoRepository _medicoRepository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly ICitaService _citaService;

        public CitasController(
            ICitaRepository citaRepository,
            IPacienteRepository pacienteRepository,
            IMedicoRepository medicoRepository,
            IEspecialidadRepository especialidadRepository,
            ICitaService citaService)
        {
            _citaRepository = citaRepository;
            _pacienteRepository = pacienteRepository;
            _medicoRepository = medicoRepository;
            _especialidadRepository = especialidadRepository;
            _citaService = citaService;
        }

        // GET: Citas
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, int? medicoId, int? especialidadId, string? search, bool isAjax = false)
        {
            // Por defecto, filtrar desde hoy si no se especifica filtro de fechas
            var citas = await _citaRepository.GetAllAsync(fechaInicio, fechaFin, medicoId, especialidadId, search);

            ViewBag.Medicos = await _medicoRepository.GetAllAsync();
            ViewBag.Especialidades = await _especialidadRepository.GetAllAsync();
            
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.MedicoId = medicoId;
            ViewBag.EspecialidadId = especialidadId;
            ViewBag.Search = search;

            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CitaTable", citas);
            }

            return View(citas);
        }

        // GET: Citas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cita = await _citaRepository.GetByIdAsync(id);
            if (cita == null)
            {
                return NotFound();
            }
            return View(cita);
        }

        // GET: Citas/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Pacientes = await _pacienteRepository.GetAllAsync();
            ViewBag.Medicos = await _medicoRepository.GetAllAsync(); // Solo activos
            return View(new Cita { FechaHora = DateTime.Today.AddDays(1).AddHours(9), Estado = "Pendiente" });
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cita cita)
        {
            // Validar disponibilidad en el servidor antes de guardar
            var (isAvailable, message) = await _citaService.ValidarDisponibilidadAsync(cita.MedicoId, cita.FechaHora);
            
            if (!isAvailable)
            {
                ModelState.AddModelError("FechaHora", message);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _citaRepository.AddAsync(cita);
                    TempData["SuccessMessage"] = "Cita médica agendada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al agendar la cita: " + ex.Message);
                }
            }

            ViewBag.Pacientes = await _pacienteRepository.GetAllAsync();
            ViewBag.Medicos = await _medicoRepository.GetAllAsync();
            return View(cita);
        }

        // GET: Citas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cita = await _citaRepository.GetByIdAsync(id);
            if (cita == null)
            {
                return NotFound();
            }
            ViewBag.Pacientes = await _pacienteRepository.GetAllAsync();
            ViewBag.Medicos = await _medicoRepository.GetAllAsync();
            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cita cita)
        {
            if (id != cita.Id)
            {
                return BadRequest();
            }

            // Validar disponibilidad excluyendo la propia cita actual
            var (isAvailable, message) = await _citaService.ValidarDisponibilidadAsync(cita.MedicoId, cita.FechaHora, cita.Id);
            if (!isAvailable)
            {
                ModelState.AddModelError("FechaHora", message);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _citaRepository.UpdateAsync(cita);
                    TempData["SuccessMessage"] = "Cita médica modificada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al modificar la cita: " + ex.Message);
                }
            }

            ViewBag.Pacientes = await _pacienteRepository.GetAllAsync();
            ViewBag.Medicos = await _medicoRepository.GetAllAsync();
            return View(cita);
        }

        // POST: Citas/Cancelar/5 (AJAX safe)
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var success = await _citaRepository.UpdateEstadoAsync(id, "Cancelada");
                if (success)
                {
                    return Json(new { success = true, message = "La cita ha sido cancelada exitosamente." });
                }
                return Json(new { success = false, message = "No se pudo cancelar la cita." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cancelar la cita: " + ex.Message });
            }
        }

        // API Endpoint para AJAX de validación de disponibilidad
        [HttpGet]
        public async Task<IActionResult> ValidarDisponibilidad(int medicoId, DateTime fechaHora, int? citaIdExcluir = null)
        {
            var (isAvailable, message) = await _citaService.ValidarDisponibilidadAsync(medicoId, fechaHora, citaIdExcluir);
            return Json(new { isAvailable, message });
        }
    }
}
