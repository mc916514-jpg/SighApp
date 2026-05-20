using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Repository;

namespace SighApp.Controllers
{
    public class EspecialidadesController : Controller
    {
        private readonly IEspecialidadRepository _especialidadRepository;

        public EspecialidadesController(IEspecialidadRepository especialidadRepository)
        {
            _especialidadRepository = especialidadRepository;
        }

        // GET: Especialidades
        public async Task<IActionResult> Index()
        {
            var list = await _especialidadRepository.GetAllAsync();
            return View(list);
        }

        // GET: Especialidades/Create
        public IActionResult Create()
        {
            return View(new Especialidad());
        }

        // POST: Especialidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Especialidad especialidad)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _especialidadRepository.AddAsync(especialidad);
                    TempData["SuccessMessage"] = "Especialidad creada con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear la especialidad: " + ex.Message);
                }
            }
            return View(especialidad);
        }

        // GET: Especialidades/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var especialidad = await _especialidadRepository.GetByIdAsync(id);
            if (especialidad == null)
            {
                return NotFound();
            }
            return View(especialidad);
        }

        // POST: Especialidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Especialidad especialidad)
        {
            if (id != especialidad.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _especialidadRepository.UpdateAsync(especialidad);
                    TempData["SuccessMessage"] = "Especialidad actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la especialidad: " + ex.Message);
                }
            }
            return View(especialidad);
        }

        // POST: Especialidades/Delete/5 (AJAX safe)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var especialidad = await _especialidadRepository.GetByIdAsync(id);
                if (especialidad == null)
                {
                    return Json(new { success = false, message = "La especialidad no existe." });
                }

                await _especialidadRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Especialidad eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "No se puede eliminar. Posiblemente existen médicos asignados a esta especialidad. Detalles: " + ex.Message });
            }
        }
    }
}
