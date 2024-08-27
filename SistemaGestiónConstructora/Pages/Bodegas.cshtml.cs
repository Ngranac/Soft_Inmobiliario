using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using static SistemaGestiónConstructora.Pages.EmpleadosModel;

namespace SistemaGestiónConstructora.Pages
{
    public class BodegasModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public BodegasModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IList<Material> Materiales { get; set; } = new List<Material>();
        public IList<Proveedor> Proveedores { get; set; } = new List<Proveedor>();

        public IList<Bodega> Bodegas { get; set; } = new List<Bodega>();

        public IList<MaterialBodega> MaterialesBodegas { get; set; } = new List<MaterialBodega>();

        public async Task OnGetAsync()
        {
            Materiales = new List<Material>();
            Proveedores = new List<Proveedor>();
            Bodegas = new List<Bodega>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT id_Material, nombre_material, id_Proveedor, descripcion
                    FROM Materiales";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Materiales.Add(new Material
                            {
                                IdMateriales = reader.GetInt32(0),
                                NombreMaterial = reader.GetString(1),
                                IdProveedor = reader.GetInt32(2),
                                Descripcion = reader.GetString(3)
                            });
                        }
                    }
                }

                string querys = "SELECT id_Proveedor, nombre_proveedor FROM Proveedores";

                using (SqlCommand command = new SqlCommand(querys, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Proveedores.Add(new Proveedor
                            {
                                IdProveedor = reader.GetInt32(0),
                                NombreProveedor = reader.GetString(1)
                            });
                        }
                    }
                }

                string query2 = @"
            SELECT mb.id_Material_Bodega, m.nombre_material, b.nombreBodega, mb.cantidad
            FROM Material_Bodegas mb
            INNER JOIN Materiales m ON mb.id_Material = m.id_Material
            INNER JOIN Bodegas b ON mb.id_Bodega = b.id_bodega";

                using (SqlCommand command = new SqlCommand(query2, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            MaterialesBodegas.Add(new MaterialBodega
                            {
                                Id = reader.GetInt32(0),
                                NombreMaterial = reader.GetString(1),
                                NombreBodega = reader.GetString(2),
                                Cantidad = reader.GetInt32(3)
                            });
                        }
                    }
                }

                string query3 = "SELECT id_bodega, nombreBodega, codigo_Postal FROM Bodegas";

                using (SqlCommand command = new SqlCommand(query3, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Bodegas.Add(new Bodega
                            {
                                IdBodega = reader.GetInt32(0),
                                NombreBodega = reader.GetString(1),
                            });
                        }
                    }
                }

            }
        }

        public async Task<IActionResult> OnPostCreateAsync(
            int idMaterial,
            string nombreMaterial,
            int idProveedor,
            string descripcion)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarMaterial @id_Material, @nombre_material, @id_Proveedor, @descripcion";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Material", idMaterial);
                    command.Parameters.AddWithValue("@nombre_material", nombreMaterial);
                    command.Parameters.AddWithValue("@id_Proveedor", idProveedor);
                    command.Parameters.AddWithValue("@descripcion", descripcion);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateMaterialBodegaAsync(
          int idMaterialBodega,
          int idMaterial,
          int idBodega,
          int cantidad)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarMaterialBodega @id_Material_Bodega, @id_Material, @id_bodega, @cantidad";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Material_Bodega", idMaterialBodega);
                    command.Parameters.AddWithValue("@id_Material", idMaterial);
                    command.Parameters.AddWithValue("@id_bodega", idBodega);
                    command.Parameters.AddWithValue("@cantidad", cantidad);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }
    }
    public class Material
    {
        public int IdMateriales { get; set; }
        public string NombreMaterial { get; set; }
        public int IdProveedor { get; set; }
        public string Descripcion { get; set; }
    }


    public class MaterialBodega
    {
        public int Id { get; set; }
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; }
        public string NombreBodega { get; set; }

        public int IdBodega { get; set; }
        public int Cantidad { get; set; }
    }

    public class Bodega
    {
        public int IdBodega { get; set; }          // Identificador único de la bodega
        public string NombreBodega { get; set; }   // Nombre de la bodega
    }

}