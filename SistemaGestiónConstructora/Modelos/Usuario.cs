namespace SistemaGestiónConstructora.Modelos
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public int IdTipoUsuario { get; set; }
        public string Contrasenna { get; set; }
        public int IdEstado { get; set; }
    }
}
