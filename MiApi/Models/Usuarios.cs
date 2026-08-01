using MiApi.Models;

public class Usuarios
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string ContrasenaHash { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public string Estado { get; set; } = "Activo";

    public ICollection<UsuarioRol> UsuariosRoles
        = new List<UsuarioRol>();

    public ICollection<Restaurante> Restaurantes
        = new List<Restaurante>();

    public ICollection<Pedido> Pedidos
        = new List<Pedido>();

    public ICollection<Direccion> Direcciones
        = new List<Direccion>();

    public ICollection<Carritos> Carritos
        = new List<Carritos>();

    public Repartidor? Repartidor { get; set; }
}