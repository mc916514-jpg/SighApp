using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class MedicoRepository : IMedicoRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public MedicoRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Medico>> GetAllAsync(int? especialidadId = null, string? search = null)
        {
            var list = new List<Medico>();
            string query = @"SELECT m.Id, m.Nombre, m.Apellido, m.Cedula, m.EspecialidadId, 
                                    e.Nombre as NombreEspecialidad, m.Telefono, m.Email, m.Activo 
                             FROM Medicos m 
                             INNER JOIN Especialidades e ON m.EspecialidadId = e.Id";

            var filters = new List<string>();

            if (especialidadId.HasValue)
            {
                filters.Add("m.EspecialidadId = @EspecialidadId");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filters.Add("(m.Nombre LIKE @Search OR m.Apellido LIKE @Search OR m.Cedula LIKE @Search OR e.Nombre LIKE @Search)");
            }

            if (filters.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filters);
            }

            query += " ORDER BY m.Apellido, m.Nombre";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
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
                        list.Add(MapMedico(reader));
                    }
                }
            }
            return list;
        }

        public async Task<Medico?> GetByIdAsync(int id)
        {
            string query = @"SELECT m.Id, m.Nombre, m.Apellido, m.Cedula, m.EspecialidadId, 
                                    e.Nombre as NombreEspecialidad, m.Telefono, m.Email, m.Activo 
                             FROM Medicos m 
                             INNER JOIN Especialidades e ON m.EspecialidadId = e.Id 
                             WHERE m.Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapMedico(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Medico medico)
        {
            string query = @"INSERT INTO Medicos (Nombre, Apellido, Cedula, EspecialidadId, Telefono, Email, Activo) 
                             VALUES (@Nombre, @Apellido, @Cedula, @EspecialidadId, @Telefono, @Email, @Activo);
                             SELECT SCOPE_IDENTITY();";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Nombre", medico.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", medico.Apellido);
                cmd.Parameters.AddWithValue("@Cedula", medico.Cedula);
                cmd.Parameters.AddWithValue("@EspecialidadId", medico.EspecialidadId);
                cmd.Parameters.AddWithValue("@Telefono", medico.Telefono);
                cmd.Parameters.AddWithValue("@Email", medico.Email);
                cmd.Parameters.AddWithValue("@Activo", medico.Activo);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Medico medico)
        {
            string query = @"UPDATE Medicos 
                             SET Nombre = @Nombre, Apellido = @Apellido, Cedula = @Cedula, 
                                 EspecialidadId = @EspecialidadId, Telefono = @Telefono, 
                                 Email = @Email, Activo = @Activo 
                             WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", medico.Id);
                cmd.Parameters.AddWithValue("@Nombre", medico.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", medico.Apellido);
                cmd.Parameters.AddWithValue("@Cedula", medico.Cedula);
                cmd.Parameters.AddWithValue("@EspecialidadId", medico.EspecialidadId);
                cmd.Parameters.AddWithValue("@Telefono", medico.Telefono);
                cmd.Parameters.AddWithValue("@Email", medico.Email);
                cmd.Parameters.AddWithValue("@Activo", medico.Activo);

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Medicos WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        private Medico MapMedico(SqlDataReader reader)
        {
            return new Medico
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellido = reader.GetString(2),
                Cedula = reader.GetString(3),
                EspecialidadId = reader.GetInt32(4),
                NombreEspecialidad = reader.GetString(5),
                Telefono = reader.GetString(6),
                Email = reader.GetString(7),
                Activo = reader.GetBoolean(8)
            };
        }
    }
}
