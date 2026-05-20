using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SighApp.Data;
using SighApp.Models;
using SighApp.Repository;

namespace SighApp.Controllers
{
    public class DiagnosticosController : Controller
    {
        private readonly IDiagnosticoRepository _diagnosticoRepository;
        private readonly ITratamientoRepository _tratamientoRepository;
        private readonly ICitaRepository _citaRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly SqlDatabaseConnection _dbConnection;

        public DiagnosticosController(
            IDiagnosticoRepository diagnosticoRepository,
            ITratamientoRepository tratamientoRepository,
            ICitaRepository citaRepository,
            IPacienteRepository pacienteRepository,
            SqlDatabaseConnection dbConnection)
        {
            _diagnosticoRepository = diagnosticoRepository;
            _tratamientoRepository = tratamientoRepository;
            _citaRepository = citaRepository;
            _pacienteRepository = pacienteRepository;
            _dbConnection = dbConnection;
        }

        // GET: Diagnosticos/Historial/5
        public async Task<IActionResult> Historial(int pacienteId)
        {
            var paciente = await _pacienteRepository.GetByIdAsync(pacienteId);
            if (paciente == null)
            {
                return NotFound();
            }

            var historial = await _diagnosticoRepository.GetHistorialClinicoAsync(pacienteId);
            foreach (var diag in historial)
            {
                diag.Tratamientos = (List<Tratamiento>)await _tratamientoRepository.GetByDiagnosticoIdAsync(diag.Id);
            }

            ViewBag.Paciente = paciente;
            return View(historial);
        }

        // GET: Diagnosticos/Create?citaId=5
        public async Task<IActionResult> Create(int citaId)
        {
            var cita = await _citaRepository.GetByIdAsync(citaId);
            if (cita == null)
            {
                return NotFound();
            }

            // Validar si ya existe un diagnóstico para esta cita
            var diagExistente = await _diagnosticoRepository.GetByCitaIdAsync(citaId);
            if (diagExistente != null)
            {
                TempData["ErrorMessage"] = "Esta cita ya posee un diagnóstico registrado.";
                return RedirectToAction("Details", "Citas", new { id = citaId });
            }

            var diagnostico = new Diagnostico
            {
                CitaId = citaId,
                NombrePaciente = cita.NombrePaciente,
                NombreMedico = cita.NombreMedico,
                FechaCita = cita.FechaHora,
                MotivoCita = cita.Motivo,
                Tratamientos = new List<Tratamiento> { new Tratamiento() } // Fila inicial
            };

            return View(diagnostico);
        }

        // POST: Diagnosticos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Diagnostico diagnostico)
        {
            // Remover validaciones innecesarias del modelo para propiedades de lectura
            ModelState.Remove("NombrePaciente");
            ModelState.Remove("NombreMedico");
            ModelState.Remove("MotivoCita");

            if (ModelState.IsValid)
            {
                // Iniciar transacción atómica utilizando ADO.NET puro
                using (var conn = _dbConnection.CreateConnection())
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Guardar Diagnóstico
                            int diagnosticoId = 0;
                            string queryDiag = @"INSERT INTO Diagnosticos (CitaId, Descripcion, FechaDiagnostico) 
                                                 VALUES (@CitaId, @Descripcion, @FechaDiagnostico);
                                                 SELECT SCOPE_IDENTITY();";

                            using (var cmdDiag = new SqlCommand(queryDiag, conn, transaction))
                            {
                                cmdDiag.Parameters.AddWithValue("@CitaId", diagnostico.CitaId);
                                cmdDiag.Parameters.AddWithValue("@Descripcion", diagnostico.Descripcion);
                                cmdDiag.Parameters.AddWithValue("@FechaDiagnostico", DateTime.Now);
                                
                                diagnosticoId = Convert.ToInt32(await cmdDiag.ExecuteScalarAsync());
                            }

                            // 2. Guardar Tratamientos asociados si los hay
                            if (diagnostico.Tratamientos != null)
                            {
                                foreach (var trat in diagnostico.Tratamientos)
                                {
                                    // Ignorar si el tratamiento está vacío
                                    if (string.IsNullOrWhiteSpace(trat.Medicamento))
                                    {
                                        continue;
                                    }

                                    string queryTrat = @"INSERT INTO Tratamientos (DiagnosticoId, Medicamento, Dosis, Frecuencia, Duracion) 
                                                         VALUES (@DiagnosticoId, @Medicamento, @Dosis, @Frecuencia, @Duracion)";

                                    using (var cmdTrat = new SqlCommand(queryTrat, conn, transaction))
                                    {
                                        cmdTrat.Parameters.AddWithValue("@DiagnosticoId", diagnosticoId);
                                        cmdTrat.Parameters.AddWithValue("@Medicamento", trat.Medicamento);
                                        cmdTrat.Parameters.AddWithValue("@Dosis", trat.Dosis);
                                        cmdTrat.Parameters.AddWithValue("@Frecuencia", trat.Frecuencia);
                                        cmdTrat.Parameters.AddWithValue("@Duracion", trat.Duracion);

                                        await cmdTrat.ExecuteNonQueryAsync();
                                    }
                                }
                            }

                            // 3. Actualizar Cita a estado 'Realizada'
                            using (var cmdCita = new SqlCommand("UPDATE Citas SET Estado = 'Realizada' WHERE Id = @CitaId", conn, transaction))
                            {
                                cmdCita.Parameters.AddWithValue("@CitaId", diagnostico.CitaId);
                                await cmdCita.ExecuteNonQueryAsync();
                            }

                            // Confirmar todos los cambios
                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "Diagnóstico e historial clínico registrados con éxito.";

                            // Obtener ID del paciente para redirigir a su historial clínico
                            var citaObj = await _citaRepository.GetByIdAsync(diagnostico.CitaId);
                            return RedirectToAction(nameof(Historial), new { pacienteId = citaObj?.PacienteId });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError("", "Error durante el registro de historial en la BD: " + ex.Message);
                        }
                    }
                }
            }

            // Recargar datos de apoyo en caso de error
            var citaError = await _citaRepository.GetByIdAsync(diagnostico.CitaId);
            if (citaError != null)
            {
                diagnostico.NombrePaciente = citaError.NombrePaciente;
                diagnostico.NombreMedico = citaError.NombreMedico;
                diagnostico.FechaCita = citaError.FechaHora;
                diagnostico.MotivoCita = citaError.Motivo;
            }

            return View(diagnostico);
        }

        // GET: Diagnosticos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var diagnostico = await _diagnosticoRepository.GetByIdAsync(id);
            if (diagnostico == null)
            {
                return NotFound();
            }

            diagnostico.Tratamientos = (List<Tratamiento>)await _tratamientoRepository.GetByDiagnosticoIdAsync(id);
            return View(diagnostico);
        }
    }
}
