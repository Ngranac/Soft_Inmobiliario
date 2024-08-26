using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaGestiónConstructora.Modelos;
using System.Data.SqlClient;

namespace SistemaGestiónConstructora.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IConfiguration _configuration;

        public List<Proyecto> Proyectos { get; set; }

        [BindProperty]
        public Proyecto Proyecto { get; set; }

        public DashboardModel(ILogger<DashboardModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            Proyectos = new List<Proyecto>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT id_Proyecto, nombre_proyecto, codigo_Postal, fecha_Inicio, Fecha_Finalizacion, id_Estado FROM Proyectos";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Proyectos.Add(new Proyecto
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                CodigoPostal = reader.GetInt32(2),
                                FechaInicio = reader.GetDateTime(3),
                                FechaFinalizacion = reader.GetDateTime(4),
                                Estado = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostCrearProyectoAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarProyecto @id_Proyecto, @nombre_proyecto, @codigo_Postal, @fecha_Inicio, @Fecha_Finalizacion, @id_Estado";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Generar un nuevo ID para el proyecto (puedes hacerlo de diversas maneras)
                    int nuevoIdProyecto = await ObtenerNuevoIdProyectoAsync(connection);

                    command.Parameters.AddWithValue("@id_Proyecto", nuevoIdProyecto);
                    command.Parameters.AddWithValue("@nombre_proyecto", Proyecto.Nombre);
                    command.Parameters.AddWithValue("@codigo_Postal", Proyecto.CodigoPostal);
                    command.Parameters.AddWithValue("@fecha_Inicio", Proyecto.FechaInicio);
                    command.Parameters.AddWithValue("@Fecha_Finalizacion", Proyecto.FechaFinalizacion);
                    command.Parameters.AddWithValue("@id_Estado", Proyecto.Estado);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage(); // Recarga la página para mostrar los cambios
        }

        private async Task<int> ObtenerNuevoIdProyectoAsync(SqlConnection connection)
        {
            string query = "SELECT ISNULL(MAX(id_Proyecto), 0) + 1 FROM Proyectos";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task<IActionResult> OnPostActualizarProyectoAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_ActualizarProyecto @id_Proyecto, @nombre_proyecto, @codigo_Postal, @fecha_Inicio, @Fecha_Finalizacion, @id_Estado";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Proyecto", Proyecto.Id);
                    command.Parameters.AddWithValue("@nombre_proyecto", Proyecto.Nombre);
                    command.Parameters.AddWithValue("@codigo_Postal", Proyecto.CodigoPostal);
                    command.Parameters.AddWithValue("@fecha_Inicio", Proyecto.FechaInicio);
                    command.Parameters.AddWithValue("@Fecha_Finalizacion", Proyecto.FechaFinalizacion);
                    command.Parameters.AddWithValue("@id_Estado", Proyecto.Estado);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage(); // Recarga la página para mostrar los cambios
        }
    }

    public class Proyecto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CodigoPostal { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinalizacion { get; set; }
        public int Estado { get; set; }
    }
}
