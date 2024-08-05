using System;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/test-connection", async (HttpContext context) =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await context.Response.WriteAsync("Conexión exitosa a la base de datos.");
        }
    }
    catch (Exception ex)
    {
        await context.Response.WriteAsync($"Error al conectar a la base de datos: {ex.Message}");
    }
});

app.Run();


