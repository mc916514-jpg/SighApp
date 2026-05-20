using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Models;
using SighApp.Repository;

namespace SighApp.Controllers
{
    public class TratamientosController : Controller
    {
        private readonly ITratamientoRepository _tratamientoRepository;

        public TratamientosController(ITratamientoRepository tratamientoRepository)
        {
            _tratamientoRepository = tratamientoRepository;
        }

        // GET: Tratamientos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tratamiento = await _tratamientoRepository.GetByIdAsync(id);
            if (tratamiento == null)
            {
                return NotFound();
            }
            return View(tratamiento);
        }

        // GET: Tratamientos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tratamiento = await _tratamientoRepository.GetByIdAsync(id);
            if (tratamiento == null)
            {
                return NotFound();
            }
            return View(tratamiento);
        }

        // POST: Tratamientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tratamiento tratamiento)
        {
            if (id != tratamiento.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _tratamientoRepository.UpdateAsync(tratamiento);
                    TempData["SuccessMessage"] = "Prescripción de tratamiento modificada con éxito.";
                    
                    // Redirigir de regreso al historial
                    var diagId = tratamiento.DiagnosticoId;
                    return RedirectToAction("Details", "Diagnosticos", new { id = diagId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al modificar la prescripción: " + ex.Message);
                }
            }
            return View(tratamiento);
        }
    }
}
