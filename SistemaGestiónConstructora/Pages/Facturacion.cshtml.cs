using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace SistemaGestiónConstructora.Pages
{
    public class FacturacionModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public FacturacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IList<Factura> Facturas { get; set; } = new List<Factura>();
        public IList<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
        public IList<TipoFactura> TiposFactura { get; set; } = new List<TipoFactura>();
        public IList<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();

        public async Task OnGetAsync()
        {
            Facturas = new List<Factura>();
            TiposFactura = new List<TipoFactura>();
            Proveedores = new List<Proveedor>();
            OrdenesCompra = new List<OrdenCompra>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consultar Facturas
                string queryFacturas = @"
                    SELECT 
                        f.id_Factura, f.nombre_factura, tf.nombre AS nombre_tipoFactura, 
                        f.fecha_emision, f.fecha_Vencimiento, f.descripcion, 
                        p.nombre_proveedor, p.correo_proveedor, p.telefono_proveedor,
                        oc.total, oc.cantidad, oc.producto, oc.precio_unitario, oc.informacion_adicional AS informacion_ordenCompra,
                        ds.descripcion_producto AS descripcion_detalleServicio
                    FROM Facturas f
                    JOIN Tipo_Facturas tf ON f.id_tipoFactura = tf.id_tipoFactura
                    JOIN Proveedores p ON f.id_Proveedor = p.id_Proveedor
                    JOIN Ordenes_Compra oc ON f.id_OrdenCompra = oc.id_OrdenCompra
                    JOIN Detalle_Servicio ds ON f.id_DetalleServicio = ds.id_DetalleServicio";

                using (SqlCommand command = new SqlCommand(queryFacturas, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Facturas.Add(new Factura
                            {
                                //IdFactura = reader.GetInt32(0),
                                //NombreFactura = reader.GetString(1),
                                //NombreTipoFactura = reader.GetString(2),
                                //FechaEmision = reader.GetDateTime(3),
                                //FechaVencimiento = reader.GetDateTime(4),
                                //Descripcion = reader.GetString(5),
                                //NombreProveedor = reader.GetString(6),
                                //CorreoProveedor = reader.GetString(7),
                                //TelefonoProveedor = reader.GetString(8),
                                //InformacionOrdenCompra = $"Total: {reader.GetDecimal(9)}, Cantidad: {reader.GetInt32(10)}, Producto: {reader.GetString(11)}, Precio Unitario: {reader.GetDecimal(12)}",
                                //DescripcionDetalleServicio = reader.GetString(13)


                                IdFactura = reader.GetInt32(reader.GetOrdinal("id_Factura")),
                                NombreFactura = reader.GetString(reader.GetOrdinal("nombre_factura")),
                                NombreTipoFactura = reader.GetString(reader.GetOrdinal("nombre_tipoFactura")),
                                FechaEmision = reader.GetDateTime(reader.GetOrdinal("fecha_emision")),
                                FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("fecha_Vencimiento")),
                                Descripcion = reader.GetString(reader.GetOrdinal("descripcion")),
                                NombreProveedor = reader.GetString(reader.GetOrdinal("nombre_proveedor")),
                                CorreoProveedor = reader.GetString(reader.GetOrdinal("correo_proveedor")),

                                // Convertir el teléfono a string
                                TelefonoProveedor = reader.GetInt32(reader.GetOrdinal("telefono_proveedor")).ToString(),

                                // Leer y convertir los valores correctamente según su tipo
                                InformacionOrdenCompra = $"Total: {Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("total")))}<br>Cantidad: {reader.GetInt32(reader.GetOrdinal("cantidad"))}<br>Producto: {reader.GetInt32(reader.GetOrdinal("producto"))}<br>Precio Unitario: {Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("precio_unitario")))}",

                                DescripcionDetalleServicio = reader.GetString(reader.GetOrdinal("descripcion_detalleServicio"))
                            });
                        }
                    }
                }

                // Consultar Proveedores
                string queryProveedores = "SELECT id_Proveedor, nombre_proveedor FROM Proveedores";
                using (SqlCommand command = new SqlCommand(queryProveedores, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
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

                // Consultar Tipos de Factura
                string queryTiposFactura = "SELECT id_tipoFactura, nombre FROM Tipo_Facturas";
                using (SqlCommand command = new SqlCommand(queryTiposFactura, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            TiposFactura.Add(new TipoFactura
                            {
                                IdTipoFactura = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            });
                        }
                    }
                }

                // Consultar Ordenes de Compra disponibles
                string queryOrdenesCompra = @"
                    SELECT oc.id_OrdenCompra, oc.total 
                    FROM Ordenes_Compra oc
                    LEFT JOIN Facturas f ON oc.id_OrdenCompra = f.id_OrdenCompra
                    WHERE f.id_Factura IS NULL";

                using (SqlCommand command = new SqlCommand(queryOrdenesCompra, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            OrdenesCompra.Add(new OrdenCompra
                            {
                                IdOrdenCompra = reader.GetInt32(0),
                                Total = (decimal)reader.GetDouble(1),
                            });
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostAgregarFacturaAsync(
     int idFactura,
     string nombreFactura,
     int idTipoFactura,
     DateTime fechaEmision,
     DateTime fechaVencimiento,
     string descripcion,
     int idProveedor,
     int idOrdenCompra,
     int idDetalleServicio)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "EXEC sp_InsertarFactura @id_Factura, @nombre_factura, @id_tipoFactura, @fecha_emision, @fecha_Vencimiento, @descripcion, @id_Proveedor, @id_OrdenCompra, @id_DetalleServicio";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Factura", idFactura);
                    command.Parameters.AddWithValue("@nombre_factura", nombreFactura);
                    command.Parameters.AddWithValue("@id_tipoFactura", idTipoFactura);
                    command.Parameters.AddWithValue("@fecha_emision", fechaEmision);
                    command.Parameters.AddWithValue("@fecha_Vencimiento", fechaVencimiento);
                    command.Parameters.AddWithValue("@descripcion", descripcion);
                    command.Parameters.AddWithValue("@id_Proveedor", idProveedor);
                    command.Parameters.AddWithValue("@id_OrdenCompra", idOrdenCompra);
                    command.Parameters.AddWithValue("@id_DetalleServicio", idDetalleServicio);

                    await command.ExecuteNonQueryAsync();
                }
            }
            
            return RedirectToPage();
        }
    }

    // Clases de modelo para Facturación
    public class Factura
    {
        public int IdFactura { get; set; }
        public string NombreFactura { get; set; }
        public int IdTipoFactura { get; set; }
        public string NombreTipoFactura { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Descripcion { get; set; }
        public string NombreProveedor { get; set; }
        public string CorreoProveedor { get; set; }
        public string TelefonoProveedor { get; set; }
        public string InformacionOrdenCompra { get; set; }
        public string DescripcionDetalleServicio { get; set; }
        public int IdProveedor { get; set; }
        public int IdOrdenCompra { get; set; }
        public int IdDetalleServicio { get; set; }
    }

    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; }
    }

    public class TipoFactura
    {
        public int IdTipoFactura { get; set; }
        public string Nombre { get; set; }
    }

    public class OrdenCompra
    {
        public int IdOrdenCompra { get; set; }
        public decimal Total { get; set; }
    }
}