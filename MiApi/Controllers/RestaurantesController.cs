using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantesController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public RestaurantesController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/restaurantes
        // GET: api/restaurantes?buscar=pizza
        // GET: api/restaurantes?idPropietario=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestauranteRespuestaDto>>>
            ObtenerRestaurantes(
                [FromQuery] string? buscar,
                [FromQuery] int? idPropietario)
        {
            var consulta = _context.Restaurantes
                .AsNoTracking()
                .AsQueryable();

            if (idPropietario.HasValue)
            {
                consulta = consulta.Where(restaurante =>
                    restaurante.IdUsuarioPropietario ==
                    idPropietario.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consulta = consulta.Where(restaurante =>
                    restaurante.Nombre.Contains(texto) ||
                    (restaurante.Descripcion != null &&
                     restaurante.Descripcion.Contains(texto)) ||
                    (restaurante.Direccion != null &&
                     restaurante.Direccion.Contains(texto)));
            }

            var restaurantes = await consulta
                .OrderBy(restaurante => restaurante.Nombre)
                .Select(restaurante => new RestauranteRespuestaDto
                {
                    IdRestaurante = restaurante.IdRestaurante,
                    IdUsuarioPropietario =
                        restaurante.IdUsuarioPropietario,
                    Propietario =
                        restaurante.UsuarioPropietario.Nombre,
                    Nombre = restaurante.Nombre,
                    Descripcion = restaurante.Descripcion,
                    Direccion = restaurante.Direccion,
                    Telefono = restaurante.Telefono,
                    HorarioApertura =
                        restaurante.HorarioApertura,
                    HorarioCierre =
                        restaurante.HorarioCierre,
                    Imagen = restaurante.Imagen,
                    Latitud = restaurante.Latitud,
                    Longitud = restaurante.Longitud,
                    CantidadProductos =
                        restaurante.Productos.Count(),
                    CantidadPedidos =
                        restaurante.Pedidos.Count(),
                    CantidadCarritos =
                        restaurante.Carritos.Count()
                })
                .ToListAsync();

            return Ok(restaurantes);
        }

        // GET: api/restaurantes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RestauranteRespuestaDto>>
            ObtenerRestaurantePorId(int id)
        {
            var restaurante = await _context.Restaurantes
                .AsNoTracking()
                .Where(restaurante =>
                    restaurante.IdRestaurante == id)
                .Select(restaurante => new RestauranteRespuestaDto
                {
                    IdRestaurante = restaurante.IdRestaurante,
                    IdUsuarioPropietario =
                        restaurante.IdUsuarioPropietario,
                    Propietario =
                        restaurante.UsuarioPropietario.Nombre,
                    Nombre = restaurante.Nombre,
                    Descripcion = restaurante.Descripcion,
                    Direccion = restaurante.Direccion,
                    Telefono = restaurante.Telefono,
                    HorarioApertura =
                        restaurante.HorarioApertura,
                    HorarioCierre =
                        restaurante.HorarioCierre,
                    Imagen = restaurante.Imagen,
                    Latitud = restaurante.Latitud,
                    Longitud = restaurante.Longitud,
                    CantidadProductos =
                        restaurante.Productos.Count(),
                    CantidadPedidos =
                        restaurante.Pedidos.Count(),
                    CantidadCarritos =
                        restaurante.Carritos.Count()
                })
                .FirstOrDefaultAsync();

            if (restaurante is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {id}."
                });
            }

            return Ok(restaurante);
        }

        // GET: api/restaurantes/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult>
            ObtenerRestauranteConRelaciones(int id)
        {
            var restaurante = await _context.Restaurantes
                .AsNoTracking()
                .Where(restaurante =>
                    restaurante.IdRestaurante == id)
                .Select(restaurante => new
                {
                    restaurante.IdRestaurante,
                    restaurante.IdUsuarioPropietario,
                    restaurante.Nombre,
                    restaurante.Descripcion,
                    restaurante.Direccion,
                    restaurante.Telefono,
                    restaurante.HorarioApertura,
                    restaurante.HorarioCierre,
                    restaurante.Imagen,
                    restaurante.Latitud,
                    restaurante.Longitud,

                    Propietario = new
                    {
                        restaurante.UsuarioPropietario.IdUsuario,
                        restaurante.UsuarioPropietario.Nombre,
                        restaurante.UsuarioPropietario.Email,
                        restaurante.UsuarioPropietario.Telefono
                    },

                    Productos = restaurante.Productos
                        .Select(producto => new
                        {
                            producto.IdProducto,
                            producto.IdCategoria,
                            Categoria = producto.Categoria.Nombre,
                            producto.Nombre,
                            producto.Descripcion,
                            producto.Precio,
                            producto.Imagen,
                            producto.Disponible,
                            
                        }),

                    Pedidos = restaurante.Pedidos
                        .Select(pedido => new
                        {
                            pedido.IdPedido,
                            pedido.NumeroPedido,
                            pedido.FechaPedido,
                            pedido.Estado,
                            pedido.Total,
                            pedido.IdUsuario,
                            pedido.IdRepartidor
                        }),

                    Carritos = restaurante.Carritos
                        .Select(carrito => new
                        {
                            carrito.IdCarrito,
                            carrito.IdUsuario,
                            carrito.FechaCreacion,
                            carrito.Estado
                        })
                })
                .FirstOrDefaultAsync();

            if (restaurante is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {id}."
                });
            }

            return Ok(restaurante);
        }

        // GET: api/restaurantes/propietario/3
        [HttpGet("propietario/{idUsuario:int}")]
        public async Task<ActionResult<IEnumerable<RestauranteRespuestaDto>>>
            ObtenerPorPropietario(int idUsuario)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == idUsuario);

            if (!usuarioExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el usuario con ID {idUsuario}."
                });
            }

            var restaurantes = await _context.Restaurantes
                .AsNoTracking()
                .Where(restaurante =>
                    restaurante.IdUsuarioPropietario == idUsuario)
                .OrderBy(restaurante => restaurante.Nombre)
                .Select(restaurante => new RestauranteRespuestaDto
                {
                    IdRestaurante = restaurante.IdRestaurante,
                    IdUsuarioPropietario =
                        restaurante.IdUsuarioPropietario,
                    Propietario =
                        restaurante.UsuarioPropietario.Nombre,
                    Nombre = restaurante.Nombre,
                    Descripcion = restaurante.Descripcion,
                    Direccion = restaurante.Direccion,
                    Telefono = restaurante.Telefono,
                    HorarioApertura =
                        restaurante.HorarioApertura,
                    HorarioCierre =
                        restaurante.HorarioCierre,
                    Imagen = restaurante.Imagen,
                    Latitud = restaurante.Latitud,
                    Longitud = restaurante.Longitud,
                    CantidadProductos =
                        restaurante.Productos.Count(),
                    CantidadPedidos =
                        restaurante.Pedidos.Count(),
                    CantidadCarritos =
                        restaurante.Carritos.Count()
                })
                .ToListAsync();

            return Ok(restaurantes);
        }

        // POST: api/restaurantes
        [HttpPost]
        public async Task<ActionResult<RestauranteRespuestaDto>>
            CrearRestaurante(CrearRestauranteDto dto)
        {
            var propietario = await _context.Usuarios
                .FirstOrDefaultAsync(usuario =>
                    usuario.IdUsuario ==
                    dto.IdUsuarioPropietario);

            if (propietario is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"El usuario propietario con ID " +
                        $"{dto.IdUsuarioPropietario} no existe."
                });
            }

            var nombreRepetido = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdUsuarioPropietario ==
                    dto.IdUsuarioPropietario &&
                    restaurante.Nombre.ToLower() ==
                    dto.Nombre.Trim().ToLower());

            if (nombreRepetido)
            {
                return Conflict(new
                {
                    mensaje =
                        "El propietario ya tiene un restaurante " +
                        "registrado con ese nombre."
                });
            }

            var nuevoRestaurante = new Restaurante
            {
                IdUsuarioPropietario =
                    dto.IdUsuarioPropietario,
                Nombre = dto.Nombre.Trim(),
                Descripcion = LimpiarTexto(dto.Descripcion),
                Direccion = LimpiarTexto(dto.Direccion),
                Telefono = LimpiarTexto(dto.Telefono),
                HorarioApertura =
                    LimpiarTexto(dto.HorarioApertura),
                HorarioCierre =
                    LimpiarTexto(dto.HorarioCierre),
                Imagen = LimpiarTexto(dto.Imagen),
                Latitud = dto.Latitud,
                Longitud = dto.Longitud
            };

            _context.Restaurantes.Add(nuevoRestaurante);
            await _context.SaveChangesAsync();

            var restauranteCreado =
                await ObtenerRestauranteDto(
                    nuevoRestaurante.IdRestaurante);

            return CreatedAtAction(
                nameof(ObtenerRestaurantePorId),
                new { id = nuevoRestaurante.IdRestaurante },
                restauranteCreado
            );
        }

        // PUT: api/restaurantes/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<RestauranteRespuestaDto>>
            ActualizarRestaurante(
                int id,
                ActualizarRestauranteDto dto)
        {
            var restaurante = await _context.Restaurantes
                .FirstOrDefaultAsync(restaurante =>
                    restaurante.IdRestaurante == id);

            if (restaurante is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {id}."
                });
            }

            var propietarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario ==
                    dto.IdUsuarioPropietario);

            if (!propietarioExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"El usuario propietario con ID " +
                        $"{dto.IdUsuarioPropietario} no existe."
                });
            }

            var nombreRepetido = await _context.Restaurantes
                .AnyAsync(otroRestaurante =>
                    otroRestaurante.IdRestaurante != id &&
                    otroRestaurante.IdUsuarioPropietario ==
                    dto.IdUsuarioPropietario &&
                    otroRestaurante.Nombre.ToLower() ==
                    dto.Nombre.Trim().ToLower());

            if (nombreRepetido)
            {
                return Conflict(new
                {
                    mensaje =
                        "Otro restaurante del propietario ya " +
                        "utiliza ese nombre."
                });
            }

            restaurante.IdUsuarioPropietario =
                dto.IdUsuarioPropietario;
            restaurante.Nombre = dto.Nombre.Trim();
            restaurante.Descripcion =
                LimpiarTexto(dto.Descripcion);
            restaurante.Direccion =
                LimpiarTexto(dto.Direccion);
            restaurante.Telefono =
                LimpiarTexto(dto.Telefono);
            restaurante.HorarioApertura =
                LimpiarTexto(dto.HorarioApertura);
            restaurante.HorarioCierre =
                LimpiarTexto(dto.HorarioCierre);
            restaurante.Imagen =
                LimpiarTexto(dto.Imagen);
            restaurante.Latitud = dto.Latitud;
            restaurante.Longitud = dto.Longitud;

            await _context.SaveChangesAsync();

            var restauranteActualizado =
                await ObtenerRestauranteDto(
                    restaurante.IdRestaurante);

            return Ok(restauranteActualizado);
        }

        // DELETE: api/restaurantes/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarRestaurante(int id)
        {
            var restaurante = await _context.Restaurantes
                .FirstOrDefaultAsync(restaurante =>
                    restaurante.IdRestaurante == id);

            if (restaurante is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {id}."
                });
            }

            var tienePedidos = await _context.Pedidos
                .AnyAsync(pedido =>
                    pedido.IdRestaurante == id);

            var tieneCarritos = await _context.Carritos
                .AnyAsync(carrito =>
                    carrito.IdRestaurante == id);

            if (tienePedidos || tieneCarritos)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el restaurante porque " +
                        "tiene pedidos o carritos relacionados."
                });
            }

            _context.Restaurantes.Remove(restaurante);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No fue posible eliminar el restaurante " +
                        "porque tiene registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje =
                    "Restaurante eliminado correctamente."
            });
        }

        private async Task<RestauranteRespuestaDto?>
            ObtenerRestauranteDto(int idRestaurante)
        {
            return await _context.Restaurantes
                .AsNoTracking()
                .Where(restaurante =>
                    restaurante.IdRestaurante ==
                    idRestaurante)
                .Select(restaurante =>
                    new RestauranteRespuestaDto
                    {
                        IdRestaurante =
                            restaurante.IdRestaurante,
                        IdUsuarioPropietario =
                            restaurante.IdUsuarioPropietario,
                        Propietario =
                            restaurante.UsuarioPropietario.Nombre,
                        Nombre = restaurante.Nombre,
                        Descripcion =
                            restaurante.Descripcion,
                        Direccion =
                            restaurante.Direccion,
                        Telefono =
                            restaurante.Telefono,
                        HorarioApertura =
                            restaurante.HorarioApertura,
                        HorarioCierre =
                            restaurante.HorarioCierre,
                        Imagen =
                            restaurante.Imagen,
                        Latitud =
                            restaurante.Latitud,
                        Longitud =
                            restaurante.Longitud,
                        CantidadProductos =
                            restaurante.Productos.Count(),
                        CantidadPedidos =
                            restaurante.Pedidos.Count(),
                        CantidadCarritos =
                            restaurante.Carritos.Count()
                    })
                .FirstOrDefaultAsync();
        }

        private static string? LimpiarTexto(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? null
                : texto.Trim();
        }
    }
}