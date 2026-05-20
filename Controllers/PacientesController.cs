using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Repository;

namespace SighApp.Controllers
{
    public class PacientesController : Controller
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IDiagnosticoRepository _diagnosticoRepository;

        public PacientesController(IPacienteRepository pacienteRepository, IDiagnosticoRepository diagnosticoRepository)
        {
            _pacienteRepository = pacienteRepository;
            _diagnosticoRepository = diagnosticoRepository;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index(string? search, bool isAjax = false)
        {
            var pacientes = await _pacienteRepository.GetAllAsync(search);
            ViewBag.Search = search;

            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PacienteTable", pacientes);
            }

            return View(pacientes);
        }

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var paciente = await _pacienteRepository.GetByIdAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }

            // También cargamos su historial clínico
            var historial = await _diagnosticoRepository.GetHistorialClinicoAsync(id);
            ViewBag.Historial = historial;

            return View(paciente);
        }

        // GET: Pacientes/Create
        public IActionResult Create()
        {
            return View(new Paciente { FechaNacimiento = DateTime.Today.AddYears(-30) });
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Paciente paciente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar cédula única antes de guardar
                    var existe = await _pacienteRepository.GetByCedulaAsync(paciente.Cedula);
                    if (existe != null)
                    {
                        ModelState.AddModelError("Cedula", "Ya existe un paciente registrado con esta Cédula / DNI.");
                        return View(paciente);
                    }

                    paciente.FechaRegistro = DateTime.Now;
                    await _pacienteRepository.AddAsync(paciente);
                    TempData["SuccessMessage"] = "Paciente registrado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al registrar el paciente: " + ex.Message);
                }
            }
            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var paciente = await _pacienteRepository.GetByIdAsync(id);
            if (paciente == null)
            {
                return NotFound();
            }
            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Paciente paciente)
        {
            if (id != paciente.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar cédula única excluida la del paciente actual
                    var existe = await _pacienteRepository.GetByCedulaAsync(paciente.Cedula);
                    if (existe != null && existe.Id != paciente.Id)
                    {
                        ModelState.AddModelError("Cedula", "Ya existe otro paciente registrado con esta Cédula / DNI.");
                        return View(paciente);
                    }

                    await _pacienteRepository.UpdateAsync(paciente);
                    TempData["SuccessMessage"] = "Datos del paciente actualizados correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar los datos: " + ex.Message);
                }
            }
            return View(paciente);
        }

        // POST: Pacientes/Delete/5 (AJAX safe)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var paciente = await _pacienteRepository.GetByIdAsync(id);
                if (paciente == null)
                {
                    return Json(new { success = false, message = "El paciente no existe." });
                }

                await _pacienteRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Paciente eliminado correctamente del sistema." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "No se puede eliminar el paciente. Posiblemente tiene citas médicas o historial registrado. Detalles: " + ex.Message });
            }
        }
    }
}
