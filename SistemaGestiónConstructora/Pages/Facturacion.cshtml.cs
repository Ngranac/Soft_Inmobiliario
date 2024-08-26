using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace SistemaGestiónConstructora.Pages
{
    public class FacturacionModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Factura> Facturas { get; set; }
        public List<SelectListItem> Proveedores { get; set; }
        public List<SelectListItem> TiposFactura { get; set; }
        public List<SelectListItem> OrdenesCompra { get; set; }
        public Factura NuevaFactura { get; set; }

        public FacturacionModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            Facturas = new List<Factura>();
            Proveedores = new List<SelectListItem>();
            TiposFactura = new List<SelectListItem>();
            OrdenesCompra = new List<SelectListItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Consultar facturas
                string query = @"
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

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Facturas.Add(new Factura
                            {
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

                // Obtener proveedores
                string proveedorQuery = "SELECT id_Proveedor, nombre_proveedor FROM Proveedores";
                using (SqlCommand command = new SqlCommand(proveedorQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Proveedores.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(reader.GetOrdinal("id_Proveedor")).ToString(),
                                Text = reader.GetString(reader.GetOrdinal("nombre_proveedor"))
                            });
                        }
                    }
                }

                // Obtener tipos de factura
                string tipoFacturaQuery = "SELECT id_tipoFactura, nombre FROM Tipo_Facturas";
                using (SqlCommand command = new SqlCommand(tipoFacturaQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TiposFactura.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(reader.GetOrdinal("id_tipoFactura")).ToString(),
                                Text = reader.GetString(reader.GetOrdinal("nombre"))
                            });
                        }
                    }
                }

                // Obtener órdenes de compra
                string ordenCompraQuery = @"
            SELECT oc.id_OrdenCompra, oc.total 
            FROM Ordenes_Compra oc
            LEFT JOIN Facturas f ON oc.id_OrdenCompra = f.id_OrdenCompra
            WHERE f.id_Factura IS NULL";

                using (SqlCommand command = new SqlCommand(ordenCompraQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrdenesCompra.Add(new SelectListItem
                            {
                                Value = reader.GetInt32(reader.GetOrdinal("id_OrdenCompra")).ToString(),
                                Text = $"ID: {reader.GetInt32(reader.GetOrdinal("id_OrdenCompra"))} - Total: {reader.GetDouble(reader.GetOrdinal("total"))}"
                            });
                        }
                    }
                }
            }
        }

        public void OnPostAgregarFactura()
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            if (NuevaFactura == null)
            {
                ModelState.AddModelError("", "Los datos de la factura no están disponibles.");
                return;
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("sp_InsertarFactura", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@id_Factura", NuevaFactura.IdFactura);
                        command.Parameters.AddWithValue("@nombre_factura", (object)NuevaFactura.NombreFactura ?? DBNull.Value);
                        command.Parameters.AddWithValue("@id_tipoFactura", NuevaFactura.IdTipoFactura);
                        command.Parameters.AddWithValue("@fecha_emision", NuevaFactura.FechaEmision);
                        command.Parameters.AddWithValue("@fecha_Vencimiento", NuevaFactura.FechaVencimiento);
                        command.Parameters.AddWithValue("@descripcion", (object)NuevaFactura.Descripcion ?? DBNull.Value);
                        command.Parameters.AddWithValue("@id_Proveedor", NuevaFactura.IdProveedor);
                        command.Parameters.AddWithValue("@identificacion_Juridica", NuevaFactura.IdentificacionJuridica);
                        command.Parameters.AddWithValue("@id_DetalleServicio", NuevaFactura.IdDetalleServicio);
                        command.Parameters.AddWithValue("@id_OrdenCompra", NuevaFactura.IdOrdenCompra);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear la factura: {ex.Message}");
            }
        }
    }
    public class Factura
    {
        public int IdFactura { get; set; }
        public string NombreFactura { get; set; }
        public int IdTipoFactura { get; set; } 
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Descripcion { get; set; }
        public int IdProveedor { get; set; }
        public int IdentificacionJuridica { get; set; } 
        public int IdDetalleServicio { get; set; }
        public int IdOrdenCompra { get; set; } 
        public string NombreTipoFactura { get; set; }
        public string NombreProveedor { get; set; }
        public string CorreoProveedor { get; set; }
        public string TelefonoProveedor { get; set; }
        public string InformacionOrdenCompra { get; set; }
        public string DescripcionDetalleServicio { get; set; }
    }


}