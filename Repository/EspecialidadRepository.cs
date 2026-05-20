using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class EspecialidadRepository : IEspecialidadRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public EspecialidadRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Especialidad>> GetAllAsync()
        {
            var list = new List<Especialidad>();
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("SELECT Id, Nombre, Descripcion FROM Especialidades ORDER BY Nombre", conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Especialidad
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return list;
        }

        public async Task<Especialidad?> GetByIdAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("SELECT Id, Nombre, Descripcion FROM Especialidades WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Especialidad
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Especialidad especialidad)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("INSERT INTO Especialidades (Nombre, Descripcion) VALUES (@Nombre, @Descripcion); SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Nombre", especialidad.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object?)especialidad.Descripcion ?? DBNull.Value);
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Especialidad especialidad)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("UPDATE Especialidades SET Nombre = @Nombre, Descripcion = @Descripcion WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", especialidad.Id);
                cmd.Parameters.AddWithValue("@Nombre", especialidad.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object?)especialidad.Descripcion ?? DBNull.Value);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Especialidades WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
