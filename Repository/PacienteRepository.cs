using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using SighApp.Data;
using SighApp.Models;

namespace SighApp.Repository
{
    public class PacienteRepository : IPacienteRepository
    {
        private readonly SqlDatabaseConnection _dbConnection;

        public PacienteRepository(SqlDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Paciente>> GetAllAsync(string? search = null)
        {
            var list = new List<Paciente>();
            string query = "SELECT Id, Nombre, Apellido, Cedula, FechaNacimiento, Genero, Telefono, Email, Direccion, FechaRegistro FROM Pacientes";
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " WHERE Nombre LIKE @Search OR Apellido LIKE @Search OR Cedula LIKE @Search OR Email LIKE @Search ORDER BY Apellido, Nombre";
            }
            else
            {
                query += " ORDER BY Apellido, Nombre";
            }

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                }

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapPaciente(reader));
                    }
                }
            }
            return list;
        }

        public async Task<Paciente?> GetByIdAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("SELECT Id, Nombre, Apellido, Cedula, FechaNacimiento, Genero, Telefono, Email, Direccion, FechaRegistro FROM Pacientes WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapPaciente(reader);
                    }
                }
            }
            return null;
        }

        public async Task<Paciente?> GetByCedulaAsync(string cedula)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("SELECT Id, Nombre, Apellido, Cedula, FechaNacimiento, Genero, Telefono, Email, Direccion, FechaRegistro FROM Pacientes WHERE Cedula = @Cedula", conn))
            {
                cmd.Parameters.AddWithValue("@Cedula", cedula);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapPaciente(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Paciente paciente)
        {
            string query = @"INSERT INTO Pacientes (Nombre, Apellido, Cedula, FechaNacimiento, Genero, Telefono, Email, Direccion, FechaRegistro) 
                             VALUES (@Nombre, @Apellido, @Cedula, @FechaNacimiento, @Genero, @Telefono, @Email, @Direccion, @FechaRegistro);
                             SELECT SCOPE_IDENTITY();";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Nombre", paciente.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", paciente.Apellido);
                cmd.Parameters.AddWithValue("@Cedula", paciente.Cedula);
                cmd.Parameters.AddWithValue("@FechaNacimiento", paciente.FechaNacimiento);
                cmd.Parameters.AddWithValue("@Genero", paciente.Genero);
                cmd.Parameters.AddWithValue("@Telefono", paciente.Telefono);
                cmd.Parameters.AddWithValue("@Email", paciente.Email);
                cmd.Parameters.AddWithValue("@Direccion", paciente.Direccion);
                cmd.Parameters.AddWithValue("@FechaRegistro", paciente.FechaRegistro);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<bool> UpdateAsync(Paciente paciente)
        {
            string query = @"UPDATE Pacientes 
                             SET Nombre = @Nombre, Apellido = @Apellido, Cedula = @Cedula, 
                                 FechaNacimiento = @FechaNacimiento, Genero = @Genero, 
                                 Telefono = @Telefono, Email = @Email, Direccion = @Direccion 
                             WHERE Id = @Id";

            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", paciente.Id);
                cmd.Parameters.AddWithValue("@Nombre", paciente.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", paciente.Apellido);
                cmd.Parameters.AddWithValue("@Cedula", paciente.Cedula);
                cmd.Parameters.AddWithValue("@FechaNacimiento", paciente.FechaNacimiento);
                cmd.Parameters.AddWithValue("@Genero", paciente.Genero);
                cmd.Parameters.AddWithValue("@Telefono", paciente.Telefono);
                cmd.Parameters.AddWithValue("@Email", paciente.Email);
                cmd.Parameters.AddWithValue("@Direccion", paciente.Direccion);

                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var conn = _dbConnection.CreateConnection())
            using (var cmd = new SqlCommand("DELETE FROM Pacientes WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                var rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        private Paciente MapPaciente(SqlDataReader reader)
        {
            return new Paciente
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Apellido = reader.GetString(2),
                Cedula = reader.GetString(3),
                FechaNacimiento = reader.GetDateTime(4),
                Genero = reader.GetString(5),
                Telefono = reader.GetString(6),
                Email = reader.GetString(7),
                Direccion = reader.GetString(8),
                FechaRegistro = reader.GetDateTime(9)
            };
        }
    }
}
