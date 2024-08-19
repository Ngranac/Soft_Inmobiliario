using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace SistemaGestiónConstructora.Pages
{
    public class RegistrarseModel : PageModel
    {
        private readonly ILogger<RegistrarseModel> _logger;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public string Cedula { get; set; }
        [BindProperty]
        public string Correo { get; set; }
        [BindProperty]
        public string Contrasenna { get; set; }

        public RegistrarseModel(ILogger<RegistrarseModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Cedula) || string.IsNullOrEmpty(Correo) || string.IsNullOrEmpty(Contrasenna))
            {
                ModelState.AddModelError(string.Empty, "Todos los campos son obligatorios.");
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarUsuario @id_Usuario, @id_TipoUsuario, @id_Estado, @correo, @contrasenna";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Usuario", Cedula);
                    command.Parameters.AddWithValue("@id_TipoUsuario", 1);
                    command.Parameters.AddWithValue("@id_Estado", 1);
                    command.Parameters.AddWithValue("@correo", Correo);
                    command.Parameters.AddWithValue("@contrasenna", Contrasenna);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage("Index");
        }
    }
}