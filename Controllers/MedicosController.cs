using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Repository;

namespace SighApp.Controllers
{
    public class MedicosController : Controller
    {
        private readonly IMedicoRepository _medicoRepository;
        private readonly IEspecialidadRepository _especialidadRepository;

        public MedicosController(IMedicoRepository medicoRepository, IEspecialidadRepository especialidadRepository)
        {
            _medicoRepository = medicoRepository;
            _especialidadRepository = especialidadRepository;
        }

        // GET: Medicos
        public async Task<IActionResult> Index(int? especialidadId, string? search, bool isAjax = false)
        {
            var medicos = await _medicoRepository.GetAllAsync(especialidadId, search);
            var especialidades = await _especialidadRepository.GetAllAsync();

            ViewBag.Especialidades = especialidades;
            ViewBag.SelectedEspecialidadId = especialidadId;
            ViewBag.Search = search;

            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MedicoTable", medicos);
            }

            return View(medicos);
        }

        // GET: Medicos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var medico = await _medicoRepository.GetByIdAsync(id);
            if (medico == null)
            {
                return NotFound();
            }
            return View(medico);
        }

        // GET: Medicos/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Especialidades = await _especialidadRepository.GetAllAsync();
            return View(new Medico { Activo = true });
        }

        // POST: Medicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medico medico)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _medicoRepository.AddAsync(medico);
                    TempData["SuccessMessage"] = "Médico registrado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al registrar el médico: " + ex.Message);
                }
            }

            ViewBag.Especialidades = await _especialidadRepository.GetAllAsync();
            return View(medico);
        }

        // GET: Medicos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var medico = await _medicoRepository.GetByIdAsync(id);
            if (medico == null)
            {
                return NotFound();
            }
            ViewBag.Especialidades = await _especialidadRepository.GetAllAsync();
            return View(medico);
        }

        // POST: Medicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Medico medico)
        {
            if (id != medico.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _medicoRepository.UpdateAsync(medico);
                    TempData["SuccessMessage"] = "Datos del médico actualizados correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar los datos: " + ex.Message);
                }
            }

            ViewBag.Especialidades = await _especialidadRepository.GetAllAsync();
            return View(medico);
        }

        // POST: Medicos/Delete/5 (AJAX safe)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var medico = await _medicoRepository.GetByIdAsync(id);
                if (medico == null)
                {
                    return Json(new { success = false, message = "El médico no existe." });
                }

                await _medicoRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Médico eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "No se puede eliminar el médico. Posiblemente tiene citas médicas registradas en el sistema. Detalles: " + ex.Message });
            }
        }
    }
}
