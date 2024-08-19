using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SistemaGestiónConstructora.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public string Correo { get; set; }
        [BindProperty]
        public string Contrasenna { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Correo) || string.IsNullOrEmpty(Contrasenna))
            {
                ModelState.AddModelError(string.Empty, "Correo y/o contraseña son obligatorios.");
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM Usuarios WHERE correo = @Correo AND contrasenna = @Contrasenna";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Correo", Correo);
                    command.Parameters.AddWithValue("@Contrasenna", Contrasenna);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Credenciales válidas
                            return RedirectToPage("Dashboard");
                        }
                        else
                        {
                            // Credenciales inválidas
                            ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
                            return Page();
                        }
                    }
                }
            }
        }
    }
}
