using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SistemaGestiónConstructora.Pages
{
    public class EmpleadosModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EmpleadosModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IList<Empleado> Empleados { get; set; } = new List<Empleado>();
        public IList<Puesto> Puestos { get; set; } = new List<Puesto>();

        public IList<int> CodigosPostales { get; set; }

        public async Task OnGetAsync()
        {
            CodigosPostales = new List<int>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT e.id_Empleado, e.nombre_Completo, e.email_Empresa, e.salario, e.codigo_Postal, p.nombre_puesto
                    FROM Empleados e
                    JOIN Puestos p ON e.id_Puesto = p.id_Puesto";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Empleados.Add(new Empleado
                            {
                                IdEmpleado = reader.GetInt32(0),
                                NombreCompleto = reader.GetString(1),
                                EmailEmpresa = reader.GetString(2),
                                Salario = reader.GetInt32(3),
                                CodigoPostal = reader.GetInt32(4),
                                NombrePuesto = reader.GetString(5)
                            });
                        }
                    }
                }

                string queryPuestos = "SELECT id_Puesto, nombre_puesto FROM Puestos";
                using (SqlCommand command = new SqlCommand(queryPuestos, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Puestos.Add(new Puesto
                            {
                                IdPuesto = reader.GetInt32(0),
                                NombrePuesto = reader.GetString(1)
                            });
                        }
                    }
                }

                string queryCodigosPostales = "SELECT DISTINCT codigo_Postal FROM Ubicacion";
                using (SqlCommand command = new SqlCommand(queryCodigosPostales, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            CodigosPostales.Add(reader.GetInt32(0));
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostCreateAsync(
            int idEmpleado,
            int idUsuario,
            string nombreCompleto,
            string emailEmpresa,
            int salario,
            int codigoPostal,
            int idPuesto)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarEmpleado @id_Empleado, @id_Usuario, @codigo_Postal, @id_Puesto, @email_Empresa, @nombre_Completo, @salario";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Empleado", idEmpleado);
                    command.Parameters.AddWithValue("@id_Usuario", idUsuario ); 
                    command.Parameters.AddWithValue("@codigo_Postal", codigoPostal);
                    command.Parameters.AddWithValue("@id_Puesto", idPuesto);
                    command.Parameters.AddWithValue("@email_Empresa", emailEmpresa);
                    command.Parameters.AddWithValue("@nombre_Completo", nombreCompleto);
                    command.Parameters.AddWithValue("@salario", salario);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        public class Empleado
        {
            public int IdEmpleado { get; set; }
            public string NombreCompleto { get; set; }
            public string EmailEmpresa { get; set; }
            public int Salario { get; set; }
            public int CodigoPostal { get; set; }
            public string NombrePuesto { get; set; }
        }

        public class Puesto
        {
            public int IdPuesto { get; set; }
            public string NombrePuesto { get; set; }
        }
    }
}