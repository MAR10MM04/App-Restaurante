using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesPedidoController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public DetallesPedidoController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/detallespedido
        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<DetallePedidoRespuestaDto>>>
            ObtenerDetalles()
        {
            var detalles = await _context.DetallesPedido
                .AsNoTracking()
                .OrderBy(detalle => detalle.IdDetallePedido)
                .Select(detalle => new DetallePedidoRespuestaDto
                {
                    IdDetallePedido = detalle.IdDetallePedido,
                    IdPedido = detalle.IdPedido,
                    NumeroPedido = detalle.Pedido.NumeroPedido,
                    IdProducto = detalle.IdProducto,
                    Producto = detalle.Producto.Nombre,
                    Imagen = detalle.Producto.Imagen,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // GET: api/detallespedido/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DetallePedidoRespuestaDto>>
            ObtenerDetallePorId(int id)
        {
            var detalle = await ObtenerDetalleDto(id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de pedido con ID {id}."
                });
            }

            return Ok(detalle);
        }

        // GET: api/detallespedido/pedido/3
        [HttpGet("pedido/{idPedido:int}")]
        public async Task<ActionResult> ObtenerDetallesPorPedido(
            int idPedido)
        {
            var pedidoExiste = await _context.Pedidos
                .AnyAsync(pedido => pedido.IdPedido == idPedido);

            if (!pedidoExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el pedido con ID {idPedido}."
                });
            }

            var detalles = await _context.DetallesPedido
                .AsNoTracking()
                .Where(detalle => detalle.IdPedido == idPedido)
                .OrderBy(detalle => detalle.IdDetallePedido)
                .Select(detalle => new DetallePedidoRespuestaDto
                {
                    IdDetallePedido = detalle.IdDetallePedido,
                    IdPedido = detalle.IdPedido,
                    NumeroPedido = detalle.Pedido.NumeroPedido,
                    IdProducto = detalle.IdProducto,
                    Producto = detalle.Producto.Nombre,
                    Imagen = detalle.Producto.Imagen,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                })
                .ToListAsync();

            return Ok(new
            {
                idPedido,
                cantidadProductos =
                    detalles.Sum(detalle => detalle.Cantidad),
                total =
                    detalles.Sum(detalle => detalle.Subtotal),
                detalles
            });
        }

        // POST: api/detallespedido
        [HttpPost]
        public async Task<ActionResult<DetallePedidoRespuestaDto>>
            CrearDetalle(CrearDetallePedidoDto dto)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == dto.IdPedido);

            if (pedido is null)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"No existe el pedido con ID {dto.IdPedido}."
                });
            }

            if (pedido.Estado.Equals(
                    "Entregado",
                    StringComparison.OrdinalIgnoreCase) ||
                pedido.Estado.Equals(
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se pueden agregar productos a un pedido " +
                        "entregado o cancelado."
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

            if (producto.IdRestaurante != pedido.IdRestaurante)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El producto no pertenece al restaurante " +
                        "del pedido."
                });
            }

            var detalleExistente = await _context.DetallesPedido
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdPedido == dto.IdPedido &&
                    detalle.IdProducto == dto.IdProducto);

            if (detalleExistente is not null)
            {
                detalleExistente.Cantidad += dto.Cantidad;
                detalleExistente.PrecioUnitario = producto.Precio;
                detalleExistente.Subtotal =
                    detalleExistente.Cantidad *
                    detalleExistente.PrecioUnitario;

                await RecalcularTotalPedido(dto.IdPedido);
                await _context.SaveChangesAsync();

                var detalleActualizado = await ObtenerDetalleDto(
                    detalleExistente.IdDetallePedido);

                return Ok(new
                {
                    mensaje =
                        "La cantidad del producto fue actualizada.",
                    detalle = detalleActualizado
                });
            }

            var nuevoDetalle = new DetallePedido
            {
                IdPedido = dto.IdPedido,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = producto.Precio,
                Subtotal = dto.Cantidad * producto.Precio
            };

            _context.DetallesPedido.Add(nuevoDetalle);

            await _context.SaveChangesAsync();

            await RecalcularTotalPedido(dto.IdPedido);
            await _context.SaveChangesAsync();

            var detalleCreado = await ObtenerDetalleDto(
                nuevoDetalle.IdDetallePedido);

            return CreatedAtAction(
                nameof(ObtenerDetallePorId),
                new { id = nuevoDetalle.IdDetallePedido },
                detalleCreado
            );
        }

        // PUT: api/detallespedido/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<DetallePedidoRespuestaDto>>
            ActualizarCantidad(
                int id,
                ActualizarDetallePedidoDto dto)
        {
            var detalle = await _context.DetallesPedido
                .Include(detalle => detalle.Pedido)
                .Include(detalle => detalle.Producto)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetallePedido == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de pedido con ID {id}."
                });
            }

            if (detalle.Pedido.Estado.Equals(
                    "Entregado",
                    StringComparison.OrdinalIgnoreCase) ||
                detalle.Pedido.Estado.Equals(
                    "Cancelado",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar un pedido entregado " +
                        "o cancelado."
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

            // Se mantiene el precio registrado en el pedido.
            detalle.Subtotal =
                detalle.Cantidad * detalle.PrecioUnitario;

            await RecalcularTotalPedido(detalle.IdPedido);
            await _context.SaveChangesAsync();

            return Ok(await ObtenerDetalleDto(id));
        }

        // PATCH: api/detallespedido/5/incrementar
        [HttpPatch("{id:int}/incrementar")]
        public async Task<ActionResult<DetallePedidoRespuestaDto>>
            IncrementarCantidad(int id)
        {
            var detalle = await _context.DetallesPedido
                .Include(detalle => detalle.Pedido)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetallePedido == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de pedido con ID {id}."
                });
            }

            if (!PedidoModificable(detalle.Pedido.Estado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar un pedido entregado " +
                        "o cancelado."
                });
            }

            detalle.Cantidad++;

            detalle.Subtotal =
                detalle.Cantidad * detalle.PrecioUnitario;

            await RecalcularTotalPedido(detalle.IdPedido);
            await _context.SaveChangesAsync();

            return Ok(await ObtenerDetalleDto(id));
        }

        // PATCH: api/detallespedido/5/disminuir
        [HttpPatch("{id:int}/disminuir")]
        public async Task<ActionResult>
            DisminuirCantidad(int id)
        {
            var detalle = await _context.DetallesPedido
                .Include(detalle => detalle.Pedido)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetallePedido == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de pedido con ID {id}."
                });
            }

            if (!PedidoModificable(detalle.Pedido.Estado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar un pedido entregado " +
                        "o cancelado."
                });
            }

            var idPedido = detalle.IdPedido;

            if (detalle.Cantidad <= 1)
            {
                _context.DetallesPedido.Remove(detalle);

                await _context.SaveChangesAsync();

                await RecalcularTotalPedido(idPedido);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje =
                        "El producto fue eliminado del pedido."
                });
            }

            detalle.Cantidad--;

            detalle.Subtotal =
                detalle.Cantidad * detalle.PrecioUnitario;

            await RecalcularTotalPedido(idPedido);
            await _context.SaveChangesAsync();

            return Ok(await ObtenerDetalleDto(id));
        }

        // DELETE: api/detallespedido/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarDetalle(int id)
        {
            var detalle = await _context.DetallesPedido
                .Include(detalle => detalle.Pedido)
                .FirstOrDefaultAsync(detalle =>
                    detalle.IdDetallePedido == id);

            if (detalle is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el detalle de pedido con ID {id}."
                });
            }

            if (!PedidoModificable(detalle.Pedido.Estado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar un pedido entregado " +
                        "o cancelado."
                });
            }

            var idPedido = detalle.IdPedido;

            _context.DetallesPedido.Remove(detalle);

            await _context.SaveChangesAsync();

            await RecalcularTotalPedido(idPedido);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Producto eliminado del pedido correctamente."
            });
        }

        // DELETE: api/detallespedido/pedido/3
        [HttpDelete("pedido/{idPedido:int}")]
        public async Task<ActionResult> EliminarDetallesDelPedido(
            int idPedido)
        {
            var pedido = await _context.Pedidos
                .Include(pedido => pedido.Detalles)
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == idPedido);

            if (pedido is null)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el pedido con ID {idPedido}."
                });
            }

            if (!PedidoModificable(pedido.Estado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede modificar un pedido entregado " +
                        "o cancelado."
                });
            }

            if (pedido.Detalles.Count == 0)
            {
                return Ok(new
                {
                    mensaje =
                        "El pedido no contiene productos."
                });
            }

            _context.DetallesPedido.RemoveRange(pedido.Detalles);

            pedido.Total = 0;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Todos los productos del pedido fueron eliminados."
            });
        }

        private async Task RecalcularTotalPedido(int idPedido)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(pedido =>
                    pedido.IdPedido == idPedido);

            if (pedido is null)
            {
                return;
            }

            pedido.Total = await _context.DetallesPedido
                .Where(detalle => detalle.IdPedido == idPedido)
                .SumAsync(detalle => (decimal?)detalle.Subtotal)
                ?? 0;
        }

        private static bool PedidoModificable(string estado)
        {
            return !estado.Equals(
                       "Entregado",
                       StringComparison.OrdinalIgnoreCase)
                   &&
                   !estado.Equals(
                       "Cancelado",
                       StringComparison.OrdinalIgnoreCase);
        }

        private async Task<DetallePedidoRespuestaDto?>
            ObtenerDetalleDto(int idDetallePedido)
        {
            return await _context.DetallesPedido
                .AsNoTracking()
                .Where(detalle =>
                    detalle.IdDetallePedido ==
                    idDetallePedido)
                .Select(detalle =>
                    new DetallePedidoRespuestaDto
                    {
                        IdDetallePedido =
                            detalle.IdDetallePedido,

                        IdPedido =
                            detalle.IdPedido,

                        NumeroPedido =
                            detalle.Pedido.NumeroPedido,

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
                            detalle.Subtotal
                    })
                .FirstOrDefaultAsync();
        }
    }
}