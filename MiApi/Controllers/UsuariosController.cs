using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;
using System.ComponentModel.DataAnnotations;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly MyMDbContext _context;
        private readonly IPasswordHasher<Usuarios> _passwordHasher;

        public UsuariosController(
            MyMDbContext context,
            IPasswordHasher<Usuarios> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioRespuestaDto>>>
            ObtenerUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Select(usuario => new UsuarioRespuestaDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    FechaRegistro = usuario.FechaRegistro,
                    Estado = usuario.Estado,

                    Roles = usuario.UsuariosRoles
                        .Select(usuarioRol => usuarioRol.Rol.Nombre)
                        .ToList(),

                    CantidadRestaurantes =
                        usuario.Restaurantes.Count(),

                    CantidadPedidos =
                        usuario.Pedidos.Count(),

                    CantidadDirecciones =
                        usuario.Direcciones.Count(),

                    CantidadCarritos =
                        usuario.Carritos.Count(),

                    TienePerfilRepartidor =
                        usuario.Repartidor != null
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/usuarios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioRespuestaDto>>
            ObtenerUsuarioPorId(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == id)
                .Select(usuario => new UsuarioRespuestaDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    FechaRegistro = usuario.FechaRegistro,
                    Estado = usuario.Estado,

                    Roles = usuario.UsuariosRoles
                        .Select(usuarioRol => usuarioRol.Rol.Nombre)
                        .ToList(),

                    CantidadRestaurantes =
                        usuario.Restaurantes.Count(),

                    CantidadPedidos =
                        usuario.Pedidos.Count(),

                    CantidadDirecciones =
                        usuario.Direcciones.Count(),

                    CantidadCarritos =
                        usuario.Carritos.Count(),

                    TienePerfilRepartidor =
                        usuario.Repartidor != null
                })
                .FirstOrDefaultAsync();

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            return Ok(usuario);
        }

        // GET: api/usuarios/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult>
            ObtenerUsuarioConRelaciones(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == id)
                .Select(usuario => new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Telefono,
                    usuario.FechaRegistro,
                    usuario.Estado,

                    Roles = usuario.UsuariosRoles
                        .Select(usuarioRol => new
                        {
                            usuarioRol.Rol.IdRol,
                            usuarioRol.Rol.Nombre,
                            usuarioRol.Rol.Descripcion,
                            usuarioRol.FechaAsignacion
                        })
                        .ToList(),

                    Restaurantes = usuario.Restaurantes
                        .Select(restaurante => new
                        {
                            restaurante.IdRestaurante,
                            restaurante.Nombre,
                            restaurante.Direccion,
                            restaurante.Telefono
                        })
                        .ToList(),

                    Direcciones = usuario.Direcciones
                        .Select(direccion => new
                        {
                            direccion.IdDireccion,
                            direccion.Latitud,
                            direccion.Longitud
                        })
                        .ToList(),

                    Pedidos = usuario.Pedidos
                        .Select(pedido => new
                        {
                            pedido.IdPedido,
                            pedido.NumeroPedido,
                            pedido.FechaPedido,
                            pedido.Estado,
                            pedido.Total,
                            pedido.IdRestaurante
                        })
                        .ToList(),

                    Carritos = usuario.Carritos
                        .Select(carrito => new
                        {
                            carrito.IdCarrito,
                            carrito.IdRestaurante,
                            carrito.FechaCreacion,
                            carrito.Estado
                        })
                        .ToList(),

                    Repartidor = usuario.Repartidor == null
                        ? null
                        : new
                        {
                            usuario.Repartidor.IdRepartidor,
                            usuario.Repartidor.Estado
                        }
                })
                .FirstOrDefaultAsync();

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            return Ok(usuario);
        }

        // POST: api/usuarios
        // Todos los usuarios nuevos reciben automáticamente el rol Cliente.
        [HttpPost]
        public async Task<ActionResult<UsuarioRespuestaDto>>
            CrearUsuario([FromBody] CrearUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            var emailExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.Email.ToLower() == email);

            if (emailExiste)
            {
                return Conflict(new
                {
                    mensaje = "El correo ya está registrado."
                });
            }

            var rolCliente = await _context.Roles
                .FirstOrDefaultAsync(rol =>
                    rol.Nombre.ToLower() == "cliente");

            if (rolCliente is null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "No existe el rol Cliente en la base de datos."
                    });
            }

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var usuario = new Usuarios
                {
                    Nombre = dto.Nombre.Trim(),
                    Email = email,

                    Telefono = string.IsNullOrWhiteSpace(dto.Telefono)
                        ? null
                        : dto.Telefono.Trim(),

                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo"
                };

                usuario.ContrasenaHash =
                    _passwordHasher.HashPassword(
                        usuario,
                        dto.Contrasena);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var usuarioRol = new UsuarioRol
                {
                    IdUsuario = usuario.IdUsuario,
                    IdRol = rolCliente.IdRol,
                    FechaAsignacion = DateTime.UtcNow
                };

                _context.UsuariosRoles.Add(usuarioRol);
                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();

                var respuesta = new UsuarioRespuestaDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    FechaRegistro = usuario.FechaRegistro,
                    Estado = usuario.Estado,
                    Roles = new List<string>
                    {
                        rolCliente.Nombre
                    },
                    CantidadRestaurantes = 0,
                    CantidadPedidos = 0,
                    CantidadDirecciones = 0,
                    CantidadCarritos = 0,
                    TienePerfilRepartidor = false
                };

                return CreatedAtAction(
                    nameof(ObtenerUsuarioPorId),
                    new { id = usuario.IdUsuario },
                    respuesta);
            }
            catch (DbUpdateException)
            {
                await transaccion.RollbackAsync();

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "Ocurrió un error al registrar el usuario."
                    });
            }
        }

        // PUT: api/usuarios/5
        // Actualiza los datos personales, pero no modifica los roles.
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UsuarioRespuestaDto>>
            ActualizarUsuario(
                int id,
                [FromBody] ActualizarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios
                .Include(usuario => usuario.UsuariosRoles)
                    .ThenInclude(usuarioRol => usuarioRol.Rol)
                .Include(usuario => usuario.Restaurantes)
                .Include(usuario => usuario.Pedidos)
                .Include(usuario => usuario.Direcciones)
                .Include(usuario => usuario.Carritos)
                .Include(usuario => usuario.Repartidor)
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario == id);

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            var emailNormalizado =
                dto.Email.Trim().ToLowerInvariant();

            var emailEnUso = await _context.Usuarios
                .AnyAsync(otroUsuario =>
                    otroUsuario.IdUsuario != id &&
                    otroUsuario.Email.ToLower() == emailNormalizado);

            if (emailEnUso)
            {
                return Conflict(new
                {
                    mensaje =
                        "El correo ya está siendo utilizado por otro usuario."
                });
            }

            usuario.Nombre = dto.Nombre.Trim();
            usuario.Email = emailNormalizado;

            usuario.Telefono =
                string.IsNullOrWhiteSpace(dto.Telefono)
                    ? null
                    : dto.Telefono.Trim();

            usuario.Estado = dto.Estado.Trim();

            await _context.SaveChangesAsync();

            return Ok(MapearUsuario(usuario));
        }

        // PATCH: api/usuarios/5/contrasena
        [HttpPatch("{id:int}/contrasena")]
        public async Task<ActionResult>
            CambiarContrasena(
                int id,
                [FromBody] CambiarContrasenaDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario == id);

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            usuario.ContrasenaHash =
                _passwordHasher.HashPassword(
                    usuario,
                    dto.NuevaContrasena);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "La contraseña fue actualizada correctamente."
            });
        }

        // PATCH: api/usuarios/5/estado
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult>
            CambiarEstado(
                int id,
                [FromBody] CambiarEstadoUsuarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estadosPermitidos = new[]
            {
                "Activo",
                "Inactivo",
                "Suspendido"
            };

            var estado = estadosPermitidos.FirstOrDefault(
                estadoPermitido =>
                    estadoPermitido.Equals(
                        dto.Estado.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (estado is null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado enviado no es válido.",
                    estadosPermitidos
                });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario == id);

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            usuario.Estado = estado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Estado actualizado correctamente.",
                usuario.IdUsuario,
                usuario.Estado
            });
        }

        // POST: api/usuarios/5/roles
        // Asigna un rol existente a un usuario.
        [HttpPost("{id:int}/roles")]
        public async Task<ActionResult>
            AsignarRol(
                int id,
                [FromBody] AsignarRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == id);

            if (!usuarioExiste)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            var nombreRol = dto.NombreRol.Trim();

            var rol = await _context.Roles
                .FirstOrDefaultAsync(rol =>
                    rol.Nombre.ToLower() ==
                    nombreRol.ToLower());

            if (rol is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el rol '{nombreRol}'."
                });
            }

            var yaTieneRol = await _context.UsuariosRoles
                .AnyAsync(usuarioRol =>
                    usuarioRol.IdUsuario == id &&
                    usuarioRol.IdRol == rol.IdRol);

            if (yaTieneRol)
            {
                return Conflict(new
                {
                    mensaje =
                        $"El usuario ya tiene el rol {rol.Nombre}."
                });
            }

            var nuevoUsuarioRol = new UsuarioRol
            {
                IdUsuario = id,
                IdRol = rol.IdRol,
                FechaAsignacion = DateTime.UtcNow
            };

            _context.UsuariosRoles.Add(nuevoUsuarioRol);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    $"El rol {rol.Nombre} fue asignado correctamente.",
                IdUsuario = id,
                rol.IdRol,
                Rol = rol.Nombre
            });
        }

        // DELETE: api/usuarios/5/roles/2
        // Quita un rol del usuario.
        [HttpDelete("{id:int}/roles/{idRol:int}")]
        public async Task<ActionResult>
            QuitarRol(int id, int idRol)
        {
            var usuarioRol = await _context.UsuariosRoles
                .Include(usuarioRol => usuarioRol.Rol)
                .FirstOrDefaultAsync(usuarioRol =>
                    usuarioRol.IdUsuario == id &&
                    usuarioRol.IdRol == idRol);

            if (usuarioRol is null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El usuario no tiene asignado ese rol."
                });
            }

            if (usuarioRol.Rol.Nombre.Equals(
                    "Cliente",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede eliminar el rol Cliente, porque es el rol básico del usuario."
                });
            }

            _context.UsuariosRoles.Remove(usuarioRol);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    $"El rol {usuarioRol.Rol.Nombre} fue eliminado correctamente."
            });
        }

        // GET: api/usuarios/5/roles
        [HttpGet("{id:int}/roles")]
        public async Task<ActionResult>
            ObtenerRolesDeUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.IdUsuario == id)
                .Select(usuario => new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,

                    Roles = usuario.UsuariosRoles
                        .Select(usuarioRol => new
                        {
                            usuarioRol.Rol.IdRol,
                            usuarioRol.Rol.Nombre,
                            usuarioRol.Rol.Descripcion,
                            usuarioRol.FechaAsignacion
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            return Ok(usuario);
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario == id);

            if (usuario is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el usuario con ID {id}."
                });
            }

            _context.Usuarios.Remove(usuario);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el usuario porque tiene " +
                        "restaurantes, pedidos u otros registros relacionados. " +
                        "Puedes cambiar su estado a Inactivo."
                });
            }

            return Ok(new
            {
                mensaje = "Usuario eliminado correctamente."
            });
        }

        private static UsuarioRespuestaDto MapearUsuario(
            Usuarios usuario)
        {
            return new UsuarioRespuestaDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FechaRegistro = usuario.FechaRegistro,
                Estado = usuario.Estado,

                Roles = usuario.UsuariosRoles?
                    .Select(usuarioRol =>
                        usuarioRol.Rol.Nombre)
                    .ToList()
                    ?? new List<string>(),

                CantidadRestaurantes =
                    usuario.Restaurantes?.Count ?? 0,

                CantidadPedidos =
                    usuario.Pedidos?.Count ?? 0,

                CantidadDirecciones =
                    usuario.Direcciones?.Count ?? 0,

                CantidadCarritos =
                    usuario.Carritos?.Count ?? 0,

                TienePerfilRepartidor =
                    usuario.Repartidor != null
            };
        }
    }

    public class CambiarEstadoUsuarioDto
    {
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }

    public class AsignarRolDto
    {
        [Required]
        [MaxLength(50)]
        public string NombreRol { get; set; } = string.Empty;
    }
}