using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class TratamientoRepository : ITratamientoRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public TratamientoRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Tratamiento>> GetByDiagnosticoIdAsync(int diagnosticoId)
        {
            var list = new List<Tratamiento>();
            string query = "SELECT Id, DiagnosticoId, Medicamento, Dosis, Frecuencia, Duracion FROM Tratamientos WHERE DiagnosticoId = @DiagnosticoId";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@DiagnosticoId", diagnosticoId);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Tratamiento
                        {
                            Id = reader.GetInt32(0),
                            DiagnosticoId = reader.GetInt32(1),
                            Medicamento = reader.GetString(2),
                            Dosis = reader.GetString(3),
                            Frecuencia = reader.GetString(4),
                            Duracion = reader.GetString(5)
                        });
                    }
                }
            }
            return list;
        }

        public async Task<Tratamiento?> GetByIdAsync(int id)
        {
            string query = "SELECT Id, DiagnosticoId, Medicamento, Dosis, Frecuencia, Duracion FROM Tratamientos WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Tratamiento
                        {
                            Id = reader.GetInt32(0),
                            DiagnosticoId = reader.GetInt32(1),
                            Medicamento = reader.GetString(2),
                            Dosis = reader.GetString(3),
                            Frecuencia = reader.GetString(4),
                            Duracion = reader.GetString(5)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Tratamiento tratamiento)
        {
            string query = @"INSERT INTO Tratamientos (DiagnosticoId, Medicamento, Dosis, Frecuencia, Duracion) 
                             VALUES (@DiagnosticoId, @Medicamento, @Dosis, @Frecuencia, @Duracion);
                             SELECT SCOPE_IDENTITY();";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@DiagnosticoId", tratamiento.DiagnosticoId);
                cmd.Parameters.AddWithValue("@Medicamento", tratamiento.Medicamento);
                cmd.Parameters.AddWithValue("@Dosis", tratamiento.Dosis);
                cmd.Parameters.AddWithValue("@Frecuencia", tratamiento.Frecuencia);
                cmd.Parameters.AddWithValue("@Duracion", tratamiento.Duracion);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Tratamiento tratamiento)
        {
            string query = @"UPDATE Tratamientos 
                             SET Medicamento = @Medicamento, Dosis = @Dosis, Frecuencia = @Frecuencia, Duracion = @Duracion 
                             WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", tratamiento.Id);
                cmd.Parameters.AddWithValue("@Medicamento", tratamiento.Medicamento);
                cmd.Parameters.AddWithValue("@Dosis", tratamiento.Dosis);
                cmd.Parameters.AddWithValue("@Frecuencia", tratamiento.Frecuencia);
                cmd.Parameters.AddWithValue("@Duracion", tratamiento.Duracion);

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Tratamientos WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAllByDiagnosticoIdAsync(int diagnosticoId)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Tratamientos WHERE DiagnosticoId = @DiagnosticoId", conn))
            {
                cmd.Parameters.AddWithValue("@DiagnosticoId", diagnosticoId);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
    }
}
