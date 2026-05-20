using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public DashboardService(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var stats = new DashboardStats();

            using (var conn = _dbConnection.CreateConnection())
            {
                await conn.OpenAsync();

                // 1. Total Pacientes
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Pacientes", conn))
                {
                    stats.TotalPacientes = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 2. Total Médicos Activos
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Medicos WHERE Activo = 1", conn))
                {
                    stats.TotalMedicos = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 3. Citas de Hoy
                string queryHoy = "SELECT COUNT(*) FROM Citas WHERE CAST(FechaHora AS DATE) = CAST(GETDATE() AS DATE) AND Estado <> 'Cancelada'";
                using (var cmd = new SqlCommand(queryHoy, conn))
                {
                    stats.TotalCitasHoy = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 4. Citas Pendientes
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE Estado = 'Pendiente'", conn))
                {
                    stats.TotalCitasPendientes = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // 5. Citas por Especialidad
                string queryEspec = @"SELECT e.Nombre, COUNT(c.Id) as Conteo
                                      FROM Citas c
                                      INNER JOIN Medicos m ON c.MedicoId = m.Id
                                      INNER JOIN Especialidades e ON m.EspecialidadId = e.Id
                                      GROUP BY e.Nombre";
                using (var cmd = new SqlCommand(queryEspec, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string especialidad = reader.GetString(0);
                        int count = reader.GetInt32(1);
                        stats.CitasPorEspecialidad[especialidad] = count;
                    }
                }

                // 6. Citas Recientes (Top 5)
                string queryRecientes = @"SELECT TOP 5 c.Id, c.PacienteId, c.MedicoId, c.FechaHora, c.Motivo, c.Estado,
                                                 (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                                 (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                                 e.Nombre as NombreEspecialidad
                                          FROM Citas c
                                          INNER JOIN Pacientes p ON c.PacienteId = p.Id
                                          INNER JOIN Medicos m ON c.MedicoId = m.Id
                                          INNER JOIN Especialidades e ON m.EspecialidadId = e.Id
                                          ORDER BY c.FechaHora DESC";
                
                var recientes = new List<Cita>();
                using (var cmd = new SqlCommand(queryRecientes, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        recientes.Add(new Cita
                        {
                            Id = reader.GetInt32(0),
                            PacienteId = reader.GetInt32(1),
                            MedicoId = reader.GetInt32(2),
                            FechaHora = reader.GetDateTime(3),
                            Motivo = reader.GetString(4),
                            Estado = reader.GetString(5),
                            NombrePaciente = reader.GetString(6),
                            NombreMedico = reader.GetString(7),
                            NombreEspecialidad = reader.GetString(8)
                        });
                    }
                }
                stats.CitasRecientes = recientes;
            }

            return stats;
        }
    }
}
