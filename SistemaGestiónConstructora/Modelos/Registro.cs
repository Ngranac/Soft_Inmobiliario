using Microsoft.AspNetCore.Mvc;

namespace SistemaGestiónConstructora.Modelos
{
    public class Registro : Controller
    {
        public string Cedula { get; set; }
        public string Correo { get; set; }
        public string Contrasenna { get; set; }
    }
}
