using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SistemaGestiónConstructora.Modelos
{
    [Route("Modelos/[controller]")]
    [ApiController]
    public class Ubicaciones : Controller
    {
        private readonly IConfiguration _configuration;

        public Ubicaciones(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetUbicaciones([FromQuery] int? paisId, [FromQuery] int? estadoId, [FromQuery] int? ciudadId)
        {
            var ubicaciones = new List<Ubicacion>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT U.codigo_Postal
                                 FROM Ubicacion U
                                 INNER JOIN OtrasSennas O ON U.id_OtrasSennas = O.id_OtrasSennas
                                 WHERE (@paisId IS NULL OR O.id_Pais = @paisId)
                                 AND (@estadoId IS NULL OR O.id_Estado = @estadoId)
                                 AND (@ciudadId IS NULL OR O.id_Ciudad = @ciudadId)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@paisId", (object)paisId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@estadoId", (object)estadoId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ciudadId", (object)ciudadId ?? DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ubicaciones.Add(new Ubicacion
                            {
                                CodigoPostal = reader.GetInt32(0)
                            });
                        }
                    }
                }
            }

            return Ok(ubicaciones);
        }

        public class Ubicacion
        {
            public int CodigoPostal { get; set; }
        }
    }
}
