using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace SistemaGestiónConstructora.Pages
{
    public class ClientesModel : PageModel
    {
        private readonly ILogger<ClientesModel> _logger;
        private readonly IConfiguration _configuration;

        public ClientesModel(ILogger<ClientesModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IList<Cliente> Clientes { get; set; }
        public IList<int> CodigosPostales { get; set; } 

        public async Task OnGetAsync()
        {
            Clientes = new List<Cliente>();
            CodigosPostales = new List<int>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT id_Cliente, nombre_cliente, correo_cliente, telefono_cliente, codigo_Postal FROM Clientes";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Clientes.Add(new Cliente
                            {
                                IdCliente = reader.GetInt32(0),
                                NombreCliente = reader.GetString(1),
                                CorreoCliente = reader.GetString(2),
                                TelefonoCliente = reader.GetInt32(3),
                                CodigoPostal = reader.GetInt32(4)
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
        public async Task<IActionResult> OnPostCreateAsync(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarCliente @id_Cliente, @nombre_cliente, @correo_cliente, @telefono_cliente, @codigo_Postal";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Cliente", cliente.IdCliente);
                    command.Parameters.AddWithValue("@nombre_cliente", cliente.NombreCliente);
                    command.Parameters.AddWithValue("@correo_cliente", cliente.CorreoCliente);
                    command.Parameters.AddWithValue("@telefono_cliente", cliente.TelefonoCliente);
                    command.Parameters.AddWithValue("@codigo_Postal", cliente.CodigoPostal);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        public class Cliente
        {
            public int IdCliente { get; set; }
            public string NombreCliente { get; set; }
            public string CorreoCliente { get; set; }
            public int TelefonoCliente { get; set; }
            public int CodigoPostal { get; set; }
        }

    }
}
