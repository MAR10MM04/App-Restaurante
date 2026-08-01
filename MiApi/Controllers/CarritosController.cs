using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarritosController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public CarritosController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/carritos
        // GET: api/carritos?idUsuario=1
        // GET: api/carritos?idRestaurante=2
        // GET: api/carritos?estado=Activo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarritoRespuestaDto>>>
            ObtenerCarritos(
                [FromQuery] int? idUsuario,
                [FromQuery] int? idRestaurante,
                [FromQuery] string? estado)
        {
            var consulta = _context.Carritos
                .AsNoTracking()
                .AsQueryable();

            if (idUsuario.HasValue)
            {
                consulta = consulta.Where(carrito =>
                    carrito.IdUsuario == idUsuario.Value);
            }

            if (idRestaurante.HasValue)
            {
                consulta = consulta.Where(carrito =>
                    carrito.IdRestaurante == idRestaurante.Value);
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoBuscado = estado.Trim();

                consulta = consulta.Where(carrito =>
                    carrito.Estado == estadoBuscado);
            }

            var carritos = await consulta
                .OrderByDescending(carrito => carrito.FechaCreacion)
                .Select(carrito => new CarritoRespuestaDto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdUsuario = carrito.IdUsuario,
                    Usuario = carrito.Usuario.Nombre,
                    IdRestaurante = carrito.IdRestaurante,
                    Restaurante = carrito.Restaurante.Nombre,
                    FechaCreacion = carrito.FechaCreacion,
                    Estado = carrito.Estado,

                    CantidadProductos = carrito.Detalles
                        .Sum(detalle => detalle.Cantidad),

                    CantidadDetalles = carrito.Detalles.Count(),

                    Total = carrito.Detalles
                        .Sum(detalle =>
                            detalle.Cantidad *
                            detalle.PrecioUnitario)
                })
                .ToListAsync();

            return Ok(carritos);
        }

        // GET: api/carritos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CarritoRespuestaDto>>
            ObtenerCarritoPorId(int id)
        {
            var carrito = await ObtenerCarritoDto(id);

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el carrito con ID {id}."
                });
            }

            return Ok(carrito);
        }

        // GET: api/carritos/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult<CarritoDetalleRespuestaDto>>
            ObtenerCarritoConDetalles(int id)
        {
            var carrito = await _context.Carritos
                .AsNoTracking()
                .Where(carrito =>
                    carrito.IdCarrito == id)
                .Select(carrito => new CarritoDetalleRespuestaDto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdUsuario = carrito.IdUsuario,
                    Usuario = carrito.Usuario.Nombre,
                    IdRestaurante = carrito.IdRestaurante,
                    Restaurante = carrito.Restaurante.Nombre,
                    FechaCreacion = carrito.FechaCreacion,
                    Estado = carrito.Estado,

                    CantidadProductos = carrito.Detalles
                        .Sum(detalle => detalle.Cantidad),

                    Total = carrito.Detalles
                        .Sum(detalle =>
                            detalle.Cantidad *
                            detalle.PrecioUnitario),

                    Detalles = carrito.Detalles
                        .Select(detalle => new ProductoCarritoDto
                        {
                            IdDetalleCarrito =
                                detalle.IdDetalleCarrito,

                            IdProducto =
                                detalle.IdProducto,

                            Producto =
                                detalle.Producto.Nombre,

                            Imagen =
                                detalle.Producto.Imagen,

                            Cantidad =
                                detalle.Cantidad,

                            PrecioUnitario =
                                detalle.PrecioUnitario,

                            Subtotal =
                                detalle.Cantidad *
                                detalle.PrecioUnitario,

                            Disponible =
                                detalle.Producto.Disponible
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el carrito con ID {id}."
                });
            }

            return Ok(carrito);
        }

        // GET: api/carritos/usuario/1
        [HttpGet("usuario/{idUsuario:int}")]
        public async Task<ActionResult<IEnumerable<CarritoRespuestaDto>>>
            ObtenerCarritosPorUsuario(int idUsuario)
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

            var carritos = await _context.Carritos
                .AsNoTracking()
                .Where(carrito =>
                    carrito.IdUsuario == idUsuario)
                .OrderByDescending(carrito =>
                    carrito.FechaCreacion)
                .Select(carrito => new CarritoRespuestaDto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdUsuario = carrito.IdUsuario,
                    Usuario = carrito.Usuario.Nombre,
                    IdRestaurante = carrito.IdRestaurante,
                    Restaurante = carrito.Restaurante.Nombre,
                    FechaCreacion = carrito.FechaCreacion,
                    Estado = carrito.Estado,

                    CantidadProductos = carrito.Detalles
                        .Sum(detalle => detalle.Cantidad),

                    CantidadDetalles = carrito.Detalles.Count(),

                    Total = carrito.Detalles
                        .Sum(detalle =>
                            detalle.Cantidad *
                            detalle.PrecioUnitario)
                })
                .ToListAsync();

            return Ok(carritos);
        }

        // GET: api/carritos/usuario/1/activo
        [HttpGet("usuario/{idUsuario:int}/activo")]
        public async Task<ActionResult<CarritoDetalleRespuestaDto>>
            ObtenerCarritoActivoUsuario(int idUsuario)
        {
            var carrito = await _context.Carritos
                .AsNoTracking()
                .Where(carrito =>
                    carrito.IdUsuario == idUsuario &&
                    carrito.Estado == "Activo")
                .OrderByDescending(carrito =>
                    carrito.FechaCreacion)
                .Select(carrito => new CarritoDetalleRespuestaDto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdUsuario = carrito.IdUsuario,
                    Usuario = carrito.Usuario.Nombre,
                    IdRestaurante = carrito.IdRestaurante,
                    Restaurante = carrito.Restaurante.Nombre,
                    FechaCreacion = carrito.FechaCreacion,
                    Estado = carrito.Estado,

                    CantidadProductos = carrito.Detalles
                        .Sum(detalle => detalle.Cantidad),

                    Total = carrito.Detalles
                        .Sum(detalle =>
                            detalle.Cantidad *
                            detalle.PrecioUnitario),

                    Detalles = carrito.Detalles
                        .Select(detalle => new ProductoCarritoDto
                        {
                            IdDetalleCarrito =
                                detalle.IdDetalleCarrito,
                            IdProducto =
                                detalle.IdProducto,
                            Producto =
                                detalle.Producto.Nombre,
                            Imagen =
                                detalle.Producto.Imagen,
                            Cantidad =
                                detalle.Cantidad,
                            PrecioUnitario =
                                detalle.PrecioUnitario,
                            Subtotal =
                                detalle.Cantidad *
                                detalle.PrecioUnitario,
                            Disponible =
                                detalle.Producto.Disponible
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El usuario no tiene un carrito activo."
                });
            }

            return Ok(carrito);
        }

        // POST: api/carritos
        [HttpPost]
        public async Task<ActionResult<CarritoRespuestaDto>>
            CrearCarrito(CrearCarritoDto dto)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario =>
                    usuario.IdUsuario == dto.IdUsuario);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el usuario con ID {dto.IdUsuario}."
                });
            }

            var restauranteExiste = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdRestaurante ==
                    dto.IdRestaurante);

            if (!restauranteExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el restaurante con ID " +
                        $"{dto.IdRestaurante}."
                });
            }

            var carritoActivo = await _context.Carritos
                .FirstOrDefaultAsync(carrito =>
                    carrito.IdUsuario == dto.IdUsuario &&
                    carrito.Estado == "Activo");

            if (carritoActivo is not null)
            {
                if (carritoActivo.IdRestaurante ==
                    dto.IdRestaurante)
                {
                    var carritoExistente =
                        await ObtenerCarritoDto(
                            carritoActivo.IdCarrito);

                    return Ok(new
                    {
                        mensaje =
                            "El usuario ya tiene un carrito activo " +
                            "para este restaurante.",
                        carrito = carritoExistente
                    });
                }

                return Conflict(new
                {
                    mensaje =
                        "El usuario ya tiene un carrito activo de " +
                        "otro restaurante. Debes vaciarlo o cerrarlo " +
                        "antes de crear otro."
                });
            }

            var nuevoCarrito = new Carritos
            {
                IdUsuario = dto.IdUsuario,
                IdRestaurante = dto.IdRestaurante,
                FechaCreacion = DateTime.UtcNow,
                Estado = "Activo"
            };

            _context.Carritos.Add(nuevoCarrito);
            await _context.SaveChangesAsync();

            var carritoCreado = await ObtenerCarritoDto(
                nuevoCarrito.IdCarrito);

            return CreatedAtAction(
                nameof(ObtenerCarritoPorId),
                new { id = nuevoCarrito.IdCarrito },
                carritoCreado
            );
        }

        // PATCH: api/carritos/5/estado
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult>
            CambiarEstado(
                int id,
                CambiarEstadoCarritoDto dto)
        {
            var carrito = await _context.Carritos
                .FirstOrDefaultAsync(carrito =>
                    carrito.IdCarrito == id);

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el carrito con ID {id}."
                });
            }

            var estadosPermitidos = new[]
            {
                "Activo",
                "Procesado",
                "Cancelado",
                "Abandonado"
            };

            var nuevoEstado = estadosPermitidos
                .FirstOrDefault(estado =>
                    estado.Equals(
                        dto.Estado.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (nuevoEstado is null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado indicado no es válido.",
                    estadosPermitidos
                });
            }

            carrito.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Estado actualizado correctamente.",
                carrito.IdCarrito,
                carrito.Estado
            });
        }

        // DELETE: api/carritos/5/vaciar
        [HttpDelete("{id:int}/vaciar")]
        public async Task<ActionResult>
            VaciarCarrito(int id)
        {
            var carrito = await _context.Carritos
                .Include(carrito => carrito.Detalles)
                .FirstOrDefaultAsync(carrito =>
                    carrito.IdCarrito == id);

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el carrito con ID {id}."
                });
            }

            if (carrito.Detalles.Count == 0)
            {
                return Ok(new
                {
                    mensaje = "El carrito ya está vacío."
                });
            }

            _context.DetallesCarrito.RemoveRange(
                carrito.Detalles);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "El carrito fue vaciado correctamente."
            });
        }

        // DELETE: api/carritos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarCarrito(int id)
        {
            var carrito = await _context.Carritos
                .Include(carrito => carrito.Detalles)
                .FirstOrDefaultAsync(carrito =>
                    carrito.IdCarrito == id);

            if (carrito is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el carrito con ID {id}."
                });
            }

            _context.Carritos.Remove(carrito);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No fue posible eliminar el carrito porque " +
                        "tiene registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje = "Carrito eliminado correctamente."
            });
        }

        private async Task<CarritoRespuestaDto?>
            ObtenerCarritoDto(int idCarrito)
        {
            return await _context.Carritos
                .AsNoTracking()
                .Where(carrito =>
                    carrito.IdCarrito == idCarrito)
                .Select(carrito => new CarritoRespuestaDto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdUsuario = carrito.IdUsuario,
                    Usuario = carrito.Usuario.Nombre,
                    IdRestaurante = carrito.IdRestaurante,
                    Restaurante = carrito.Restaurante.Nombre,
                    FechaCreacion = carrito.FechaCreacion,
                    Estado = carrito.Estado,

                    CantidadProductos = carrito.Detalles
                        .Sum(detalle => detalle.Cantidad),

                    CantidadDetalles = carrito.Detalles.Count(),

                    Total = carrito.Detalles
                        .Sum(detalle =>
                            detalle.Cantidad *
                            detalle.PrecioUnitario)
                })
                .FirstOrDefaultAsync();
        }
    }
}