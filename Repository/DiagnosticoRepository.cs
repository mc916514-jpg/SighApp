using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class DiagnosticoRepository : IDiagnosticoRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public DiagnosticoRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Diagnostico>> GetHistorialClinicoAsync(int pacienteId)
        {
            var list = new List<Diagnostico>();
            string query = @"SELECT d.Id, d.CitaId, d.Descripcion, d.FechaDiagnostico,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    c.FechaHora as FechaCita,
                                    c.Motivo as MotivoCita
                             FROM Diagnosticos d
                             INNER JOIN Citas c ON d.CitaId = c.Id
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             WHERE c.PacienteId = @PacienteId
                             ORDER BY d.FechaDiagnostico DESC";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@PacienteId", pacienteId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapDiagnostico(reader));
                    }
                }
            }
            return list;
        }

        public async Task<Diagnostico?> GetByIdAsync(int id)
        {
            string query = @"SELECT d.Id, d.CitaId, d.Descripcion, d.FechaDiagnostico,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    c.FechaHora as FechaCita,
                                    c.Motivo as MotivoCita
                             FROM Diagnosticos d
                             INNER JOIN Citas c ON d.CitaId = c.Id
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             WHERE d.Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapDiagnostico(reader);
                    }
                }
            }
            return null;
        }

        public async Task<Diagnostico?> GetByCitaIdAsync(int citaId)
        {
            string query = @"SELECT d.Id, d.CitaId, d.Descripcion, d.FechaDiagnostico,
                                    (p.Nombre + ' ' + p.Apellido) as NombrePaciente,
                                    (m.Nombre + ' ' + m.Apellido) as NombreMedico,
                                    c.FechaHora as FechaCita,
                                    c.Motivo as MotivoCita
                             FROM Diagnosticos d
                             INNER JOIN Citas c ON d.CitaId = c.Id
                             INNER JOIN Pacientes p ON c.PacienteId = p.Id
                             INNER JOIN Medicos m ON c.MedicoId = m.Id
                             WHERE d.CitaId = @CitaId";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CitaId", citaId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapDiagnostico(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Diagnostico diagnostico)
        {
            string query = @"INSERT INTO Diagnosticos (CitaId, Descripcion, FechaDiagnostico) 
                             VALUES (@CitaId, @Descripcion, @FechaDiagnostico);
                             SELECT SCOPE_IDENTITY();";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CitaId", diagnostico.CitaId);
                cmd.Parameters.AddWithValue("@Descripcion", diagnostico.Descripcion);
                cmd.Parameters.AddWithValue("@FechaDiagnostico", diagnostico.FechaDiagnostico);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Diagnostico diagnostico)
        {
            string query = @"UPDATE Diagnosticos 
                             SET Descripcion = @Descripcion, FechaDiagnostico = @FechaDiagnostico 
                             WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", diagnostico.Id);
                cmd.Parameters.AddWithValue("@Descripcion", diagnostico.Descripcion);
                cmd.Parameters.AddWithValue("@FechaDiagnostico", diagnostico.FechaDiagnostico);

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Diagnosticos WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        private Diagnostico MapDiagnostico(SqlDataReader reader)
        {
            return new Diagnostico
            {
                Id = reader.GetInt32(0),
                CitaId = reader.GetInt32(1),
                Descripcion = reader.GetString(2),
                FechaDiagnostico = reader.GetDateTime(3),
                NombrePaciente = reader.GetString(4),
                NombreMedico = reader.GetString(5),
                FechaCita = reader.GetDateTime(6),
                MotivoCita = reader.GetString(7)
            };
        }
    }
}
