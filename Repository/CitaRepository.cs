using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class CitaRepository : ICitaRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public CitaRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Cita>> GetAllAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int? medicoId = null, int? especialidadId = null, string? search = null)
        {
            var list = new List<Cita>();
            string query = @"SELECT c.Id, c.PacienteId, c.MedicoId, c.FechaHora, c.Motivo, c.Estado,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    e.Nombre as NombreEspecialidad,
                                    p.Cedula as CedulaPaciente
                             FROM Citas c
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             INNER JOIN Especialidades e ON m.EspecialidadId = e.Id";

            var filters = new List<string>();

            if (fechaInicio.HasValue)
            {
                filters.Add("c.FechaHora >= @FechaInicio");
            }
            if (fechaFin.HasValue)
            {
                filters.Add("c.FechaHora <= @FechaFin");
            }
            if (medicoId.HasValue)
            {
                filters.Add("c.MedicoId = @MedicoId");
            }
            if (especialidadId.HasValue)
            {
                filters.Add("m.EspecialidadId = @EspecialidadId");
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                filters.Add("(p.Nombre LIKE @Search OR p.Apellido LIKE @Search OR m.Nombre LIKE @Search OR m.Apellido LIKE @Search OR c.Motivo LIKE @Search)");
            }

            if (filters.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filters);
            }

            query += " ORDER BY c.FechaHora DESC";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (fechaInicio.HasValue)
                {
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Value);
                }
                if (fechaFin.HasValue)
                {
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Value);
                }
                if (medicoId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@MedicoId", medicoId.Value);
                }
                if (especialidadId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@EspecialidadId", especialidadId.Value);
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                }

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapCita(reader));
                    }
                }
            }
            return list;
        }

        public async Task<Cita?> GetByIdAsync(int id)
        {
            string query = @"SELECT c.Id, c.PacienteId, c.MedicoId, c.FechaHora, c.Motivo, c.Estado,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    e.Nombre as NombreEspecialidad,
                                    p.Cedula as CedulaPaciente
                             FROM Citas c
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             INNER JOIN Especialidades e ON m.EspecialidadId = e.Id
                             WHERE c.Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapCita(reader);
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<Cita>> GetByMedicoAndFechaAsync(int medicoId, DateTime fecha)
        {
            var list = new List<Cita>();
            string query = @"SELECT c.Id, c.PacienteId, c.MedicoId, c.FechaHora, c.Motivo, c.Estado,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    e.Nombre as NombreEspecialidad,
                                    p.Cedula as CedulaPaciente
                             FROM Citas c
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             INNER JOIN Especialidades e ON m.EspecialidadId = e.Id
                             WHERE c.MedicoId = @MedicoId 
                               AND CAST(c.FechaHora AS DATE) = CAST(@Fecha AS DATE)
                               AND c.Estado <> 'Cancelada'";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MedicoId", medicoId);
                cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapCita(reader));
                    }
                }
            }
            return list;
        }

        public async Task<int> AddAsync(Cita cita)
        {
            string query = @"INSERT INTO Citas (PacienteId, MedicoId, FechaHora, Motivo, Estado) 
                             VALUES (@PacienteId, @MedicoId, @FechaHora, @Motivo, @Estado);
                             SELECT SCOPE_IDENTITY();";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PacienteId", cita.PacienteId);
                cmd.Parameters.AddWithValue("@MedicoId", cita.MedicoId);
                cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                cmd.Parameters.AddWithValue("@Motivo", cita.Motivo);
                cmd.Parameters.AddWithValue("@Estado", cita.Estado);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Cita cita)
        {
            string query = @"UPDATE Citas 
                             SET PacienteId = @PacienteId, MedicoId = @MedicoId, 
                                 FechaHora = @FechaHora, Motivo = @Motivo, Estado = @Estado 
                             WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", cita.Id);
                cmd.Parameters.AddWithValue("@PacienteId", cita.PacienteId);
                cmd.Parameters.AddWithValue("@MedicoId", cita.MedicoId);
                cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);
                cmd.Parameters.AddWithValue("@Motivo", cita.Motivo);
                cmd.Parameters.AddWithValue("@Estado", cita.Estado);

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> UpdateEstadoAsync(int id, string estado)
        {
            string query = "UPDATE Citas SET Estado = @Estado WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Estado", estado);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Citas WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        private Cita MapCita(SqlDataReader reader)
        {
            return new Cita
            {
                Id = reader.GetInt32(0),
                PacienteId = reader.GetInt32(1),
                MedicoId = reader.GetInt32(2),
                FechaHora = reader.GetDateTime(3),
                Motivo = reader.GetString(4),
                Estado = reader.GetString(5),
                NombrePaciente = reader.GetString(6),
                NombreMedico = reader.GetString(7),
                NombreEspecialidad = reader.GetString(8),
                CedulaPaciente = reader.GetString(9)
            };
        }
    }
}
