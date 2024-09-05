using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;
using System;
using System.Xml.Linq;

namespace WebAPIDockerUTN.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIController : ControllerBase
    {
        private readonly string _connectionString;
        public APIController()
        {
            _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");            

        }
        [HttpPost("add")]
        public async Task<IActionResult> AddStringAsync([FromBody] string value)
        {

            if (!TableExists())
            {
                var responseCatch = new
                {
                    Success = false,
                    Message = $"Tabla no creada, intente nuevamente",
                    AdditionalInfo = "Conexión a " + Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                };

                string jsonResponseCatch = JsonSerializer.Serialize(responseCatch);
                Console.WriteLine("Tabla Creada");
                return StatusCode(500, jsonResponseCatch);

            }

            if (string.IsNullOrEmpty(value))
            {
                return BadRequest("Value cannot be empty.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    Console.WriteLine("Conectando a " + _connectionString);
                    await connection.OpenAsync();
                    string query = "INSERT INTO TABLE1 (Value) VALUES (@Value)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Value", value);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                var response = new
                {
                    Success = true,
                    Message = " String added successfully.",
                    AdditionalInfo = "Conexión a " + _connectionString
                };

                string jsonResponse = JsonSerializer.Serialize(response);
                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de conexion  en " + _connectionString);
                var response = new
                {
                    Success = false,
                    Message = $" Internal server error: {ex.Message}",
                    AdditionalInfo = "Conexión a " + _connectionString
                };

                string jsonResponse = JsonSerializer.Serialize(response);

                return StatusCode(500, jsonResponse);
            }
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllStrings()
        {

            if (!TableExists())
            {
                var responseCatch = new
                {
                    Success = false,
                    Message = $"Tabla no creada, intente nuevamente",
                    AdditionalInfo = "Conexión a " + Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                };

                string jsonResponseCatch = JsonSerializer.Serialize(responseCatch);
                Console.WriteLine("Tabla Creada");
                return StatusCode(500, jsonResponseCatch);

            }
            List<string> results = new List<string>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    Console.WriteLine("Conectando a " + _connectionString);
                    await connection.OpenAsync();
                    string query = "SELECT Value FROM TABLE1";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(reader.GetString(0));
                        }
                    }
                    Console.WriteLine("Query ejecutada con exito " + query);
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en conexino a " + _connectionString);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("getDBInfo")]
        public async Task<IActionResult> getDBInfo()
        {
            int dbCount = 0;
            try
            {
                using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
                {
                    try
                    {
                        connection.Open();
                        Console.WriteLine("Conexión a la base de datos establecida correctamente.");

                        // Verificar si la tabla existe y crearla si no existe
                        bool tableExists = TableExists();

                        if (!tableExists)
                        {
                            Console.WriteLine("Tabla creada porque no existía.");
                        }
                        else
                        {
                            Console.WriteLine("La tabla ya existía.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + " " + ex.StackTrace);
                    }
                }
            }
            catch (Exception ex)
            {
                var responseCatch = new
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    AdditionalInfo = "Conexión a " + Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                };

                string jsonResponseCatch = JsonSerializer.Serialize(responseCatch);
                Console.WriteLine(ex.Message);
                return StatusCode(500, jsonResponseCatch);
            }

            var response = new
            {
                Success = true,
                Message = "",
                AdditionalInfo = "Conexión a " + Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
            };

            string jsonResponse = JsonSerializer.Serialize(response);
            return Ok(jsonResponse);
        }



        private bool TableExists()
        {
            bool exists = false;
            string tableName = "TABLE1";
            using (var connection = new SqlConnection(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Conexión a la base de datos establecida correctamente.");

                    // Verificar si la tabla existe
                    string checkTableQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                    using (SqlCommand checkTableCommand = new SqlCommand(checkTableQuery, connection))
                    {
                        int count = (int)checkTableCommand.ExecuteScalar();
                        exists = count > 0;
                    }
                    Console.WriteLine("Existe la base: "+ exists);
                    if (!exists)
                    {
                        // Crear la tabla si no existe
                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string queryFilePath = Path.Combine(baseDirectory, "Resources", "query.sql");
                        Console.WriteLine("Query Path " + queryFilePath);

                        string query = System.IO.File.ReadAllText(queryFilePath);
                        Console.WriteLine(query);

                        using (SqlCommand createTableCommand = new SqlCommand(query, connection))
                        {
                            createTableCommand.ExecuteNonQuery();
                        }
                        Console.WriteLine("Tabla creada");
                    }
                    else
                    {
                        Console.WriteLine("La tabla ya existe");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " " + ex.StackTrace);
                }
            }

            return exists;
        }


    }
}
