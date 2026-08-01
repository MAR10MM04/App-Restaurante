using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesCarritoController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public DetallesCarritoController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/detallescarrito
        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<DetalleCarritoRespuestaDto>>>
            ObtenerDetalles()
        {
            var detalles = await _context.DetallesCarrito
                .AsNoTracking()
                .OrderBy(detalle => detalle.IdDetalleCarrito)
                .Select(detalle => new DetalleCarritoRespuestaDto
                {
                    IdDetalleCarrito = detalle.IdDetalleCarrito,
                    IdCarrito = detalle.IdCarrito,
                    IdProducto = detalle.IdProducto,
                    Producto = detalle.Producto.Nombre,
                    Imagen = detalle.Producto.Imagen,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal =
                        detalle.Cantidad * detalle.PrecioUnitario,
                    Disponible = detalle.Producto.Disponible
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // GET: api/detallescarrito/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DetalleCarritoRespuestaDto>>
            ObtenerDetallePorId(int id)
        {
            var detalle = await ObtenerDetalleDto(id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de carrito con ID {id}."
                });
            }

            return Ok(detalle);
        }

        // GET: api/detallescarrito/carrito/3
        [HttpGet("carrito/{idCarrito:int}")]
        public async Task<
            ActionResult<IEnumerable<DetalleCarritoRespuestaDto>>>
            ObtenerDetallesPorCarrito(int idCarrito)
        {
            var carritoExiste = await _context.Carritos
                .AnyAsync(carrito =>
                    carrito.IdCarrito == idCarrito);

            if (!carritoExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el carrito con ID {idCarrito}."
                });
            }

            var detalles = await _context.DetallesCarrito
                .AsNoTracking()
                .Where(detalle =>
                    detalle.IdCarrito == idCarrito)
                .OrderBy(detalle => detalle.IdDetalleCarrito)
                .Select(detalle => new DetalleCarritoRespuestaDto
                {
                    IdDetalleCarrito =
                        detalle.IdDetalleCarrito,
                    IdCarrito = detalle.IdCarrito,
                    IdProducto = detalle.IdProducto,
                    Producto = detalle.Producto.Nombre,
                    Imagen = detalle.Producto.Imagen,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal =
                        detalle.Cantidad *
                        detalle.PrecioUnitario,
                    Disponible = detalle.Producto.Disponible
                })
                .ToListAsync();

            return Ok(new
            {
                idCarrito,
                cantidadProductos =
                    detalles.Sum(detalle => detalle.Cantidad),
                total =
                    detalles.Sum(detalle => detalle.Subtotal),
                detalles
            });
        }

        // POST: api/detallescarrito
        [HttpPost]
        public async Task<
            ActionResult<DetalleCarritoRespuestaDto>>
            AgregarProducto(CrearDetalleCarritoDto dto)
        {
            var carrito = await _context.Carritos
                .FirstOrDefaultAsync(carrito =>
                    carrito.IdCarrito == dto.IdCarrito);

            if (carrito is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el carrito con ID {dto.IdCarrito}."
                });
            }

            if (!carrito.Estado.Equals(
                    "Activo",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pueden agregar productos a un carrito inactivo."
                });
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(producto =>
                    producto.IdProducto == dto.IdProducto);

            if (producto is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el producto con ID {dto.IdProducto}."
                });
            }

            if (!producto.Disponible)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto seleccionado no está disponible."
                });
            }

            if (producto.IdRestaurante != carrito.IdRestaurante)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto no pertenece al restaurante del carrito."
                });
            }

            var detalleExistente =
                await _context.DetallesCarrito
                    .FirstOrDefaultAsync(detalle =>
                        detalle.IdCarrito == dto.IdCarrito &&
                        detalle.IdProducto == dto.IdProducto);

            if (detalleExistente is not null)
            {
                detalleExistente.Cantidad += dto.Cantidad;

                // Actualiza el precio al precio actual del producto.
                detalleExistente.PrecioUnitario =
                    producto.Precio;

                await _context.SaveChangesAsync();

                var detalleActualizado =
                    await ObtenerDetalleDto(
                        detalleExistente.IdDetalleCarrito);

                return Ok(new
                {
                    mensaje =
                        "La cantidad del producto fue actualizada.",
                    detalle = detalleActualizado
                });
            }

            var nuevoDetalle = new DetalleCarrito
            {
                IdCarrito = dto.IdCarrito,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = producto.Precio
            };

            _context.DetallesCarrito.Add(nuevoDetalle);
            await _context.SaveChangesAsync();

            var detalleCreado = await ObtenerDetalleDto(
                nuevoDetalle.IdDetalleCarrito);

            return CreatedAtAction(
                nameof(ObtenerDetallePorId),
                new { id = nuevoDetalle.IdDetalleCarrito },
                detalleCreado
            );
        }

        // PUT: api/detallescarrito/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<DetalleCarritoRespuestaDto>>
            ActualizarCantidad(
                int id,
                ActualizarDetalleCarritoDto dto)
        {
            var detalle = await _context.DetallesCarrito
                .Include(detalle => detalle.Producto)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetalleCarrito == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de carrito con ID {id}."
                });
            }

            if (!detalle.Producto.Disponible)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto ya no se encuentra disponible."
                });
            }

            detalle.Cantidad = dto.Cantidad;

            // Sincroniza el precio con el precio actual.
            detalle.PrecioUnitario =
                detalle.Producto.Precio;

            await _context.SaveChangesAsync();

            var detalleActualizado =
                await ObtenerDetalleDto(id);

            return Ok(detalleActualizado);
        }

        // PATCH: api/detallescarrito/5/incrementar
        [HttpPatch("{id:int}/incrementar")]
        public async Task<ActionResult<DetalleCarritoRespuestaDto>>
            IncrementarCantidad(int id)
        {
            var detalle = await _context.DetallesCarrito
                .Include(detalle => detalle.Producto)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetalleCarrito == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de carrito con ID {id}."
                });
            }

            if (!detalle.Producto.Disponible)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto ya no se encuentra disponible."
                });
            }

            detalle.Cantidad++;
            detalle.PrecioUnitario =
                detalle.Producto.Precio;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerDetalleDto(id));
        }

        // PATCH: api/detallescarrito/5/disminuir
        [HttpPatch("{id:int}/disminuir")]
        public async Task<ActionResult>
            DisminuirCantidad(int id)
        {
            var detalle = await _context.DetallesCarrito
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetalleCarrito == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de carrito con ID {id}."
                });
            }

            if (detalle.Cantidad <= 1)
            {
                _context.DetallesCarrito.Remove(detalle);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje =
                        "El producto fue eliminado del carrito."
                });
            }

            detalle.Cantidad--;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerDetalleDto(id));
        }

        // DELETE: api/detallescarrito/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult>
            EliminarDetalle(int id)
        {
            var detalle = await _context.DetallesCarrito
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetalleCarrito == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de carrito con ID {id}."
                });
            }

            _context.DetallesCarrito.Remove(detalle);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Producto eliminado del carrito correctamente."
            });
        }

        // DELETE: api/detallescarrito/carrito/3
        [HttpDelete("carrito/{idCarrito:int}")]
        public async Task<ActionResult>
            VaciarCarrito(int idCarrito)
        {
            var carritoExiste = await _context.Carritos
                .AnyAsync(carrito =>
                    carrito.IdCarrito == idCarrito);

            if (!carritoExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el carrito con ID {idCarrito}."
                });
            }

            var detalles = await _context.DetallesCarrito
                .Where(detalle =>
                    detalle.IdCarrito == idCarrito)
                .ToListAsync();

            if (detalles.Count == 0)
            {
                return Ok(new
                {
                    mensaje =
                        "El carrito ya se encuentra vacío."
                });
            }

            _context.DetallesCarrito.RemoveRange(detalles);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "El carrito fue vaciado correctamente."
            });
        }

        private async Task<DetalleCarritoRespuestaDto?>
            ObtenerDetalleDto(int idDetalle)
        {
            return await _context.DetallesCarrito
                .AsNoTracking()
                .Where(detalle =>
                    detalle.IdDetalleCarrito == idDetalle)
                .Select(detalle =>
                    new DetalleCarritoRespuestaDto
                    {
                        IdDetalleCarrito =
                            detalle.IdDetalleCarrito,
                        IdCarrito =
                            detalle.IdCarrito,
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
                .FirstOrDefaultAsync();
        }
    }
}