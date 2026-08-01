using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;
using static MiApi.DTOs.ActualizarProductoDto;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public ProductosController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/productos
        // GET: api/productos?disponible=true
        // GET: api/productos?idRestaurante=1
        // GET: api/productos?idCategoria=2
        // GET: api/productos?buscar=hamburguesa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoRespuestaDto>>>
            ObtenerProductos(
                [FromQuery] int? idRestaurante,
                [FromQuery] int? idCategoria,
                [FromQuery] bool? disponible,
                [FromQuery] string? buscar)
        {
            var consulta = _context.Productos
                .AsNoTracking()
                .AsQueryable();

            if (idRestaurante.HasValue)
            {
                consulta = consulta.Where(producto =>
                    producto.IdRestaurante == idRestaurante.Value);
            }

            if (idCategoria.HasValue)
            {
                consulta = consulta.Where(producto =>
                    producto.IdCategoria == idCategoria.Value);
            }

            if (disponible.HasValue)
            {
                consulta = consulta.Where(producto =>
                    producto.Disponible == disponible.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consulta = consulta.Where(producto =>
                    producto.Nombre.Contains(texto) ||
                    (producto.Descripcion != null &&
                     producto.Descripcion.Contains(texto)));
            }

            var productos = await consulta
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre,
                    IdCategoria = producto.IdCategoria,
                    Categoria = producto.Categoria.Nombre,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,
                   
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/productos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoRespuestaDto>>
            ObtenerProductoPorId(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Where(producto => producto.IdProducto == id)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre,
                    IdCategoria = producto.IdCategoria,
                    Categoria = producto.Categoria.Nombre,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,
                  
                })
                .FirstOrDefaultAsync();

            if (producto is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el producto con ID {id}."
                });
            }

            return Ok(producto);
        }

        // GET: api/productos/restaurante/3
        [HttpGet("restaurante/{idRestaurante:int}")]
        public async Task<ActionResult<IEnumerable<ProductoRespuestaDto>>>
            ObtenerProductosPorRestaurante(int idRestaurante)
        {
            var restauranteExiste = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdRestaurante == idRestaurante);

            if (!restauranteExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe el restaurante con ID {idRestaurante}."
                });
            }

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(producto =>
                    producto.IdRestaurante == idRestaurante)
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre,
                    IdCategoria = producto.IdCategoria,
                    Categoria = producto.Categoria.Nombre,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,
                    
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/productos/categoria/2
        [HttpGet("categoria/{idCategoria:int}")]
        public async Task<ActionResult<IEnumerable<ProductoRespuestaDto>>>
            ObtenerProductosPorCategoria(int idCategoria)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(categoria =>
                    categoria.IdCategoria == idCategoria);

            if (!categoriaExiste)
            {
                return NotFound(new
                {
                    mensaje =
                        $"No existe la categoría con ID {idCategoria}."
                });
            }

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(producto =>
                    producto.IdCategoria == idCategoria)
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre,
                    IdCategoria = producto.IdCategoria,
                    Categoria = producto.Categoria.Nombre,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,

                })
                .ToListAsync();

            return Ok(productos);
        }

// POST: api/productos/form
[HttpPost("form")]
public async Task<IActionResult> CrearProductoConImagen(
    [FromForm] CrearProductoFormDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var restauranteExiste = await _context.Restaurantes
        .AnyAsync(r => r.IdRestaurante == dto.IdRestaurante);

    if (!restauranteExiste)
    {
        return BadRequest(new
        {
            mensaje = "El restaurante no existe."
        });
    }

    var categoriaExiste = await _context.Categorias
        .AnyAsync(c => c.IdCategoria == dto.IdCategoria);

    if (!categoriaExiste)
    {
        return BadRequest(new
        {
            mensaje = "La categoría no existe."
        });
    }

    var nombreRepetido = await _context.Productos
        .AnyAsync(p =>
            p.IdRestaurante == dto.IdRestaurante &&
            p.Nombre.ToLower() == dto.Nombre.Trim().ToLower());

    if (nombreRepetido)
    {
        return Conflict(new
        {
            mensaje = "Ya existe un producto con ese nombre."
        });
    }

    string? urlImagen = null;

    if (dto.Imagen != null && dto.Imagen.Length > 0)
    {
        var carpeta = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/productos");

        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        var nombreArchivo =
            $"{Guid.NewGuid()}{Path.GetExtension(dto.Imagen.FileName)}";

        var rutaArchivo = Path.Combine(carpeta, nombreArchivo);

        using var stream = new FileStream(
            rutaArchivo,
            FileMode.Create);

        await dto.Imagen.CopyToAsync(stream);

        urlImagen = $"/productos/{nombreArchivo}";
    }

    var producto = new Producto
    {
        IdRestaurante = dto.IdRestaurante,
        IdCategoria = dto.IdCategoria,
        Nombre = dto.Nombre.Trim(),
        Descripcion = dto.Descripcion,
        Precio = dto.Precio,
        Disponible = dto.Disponible,
        
        Imagen = urlImagen
    };

    _context.Productos.Add(producto);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        mensaje = "Producto creado correctamente.",
        producto.IdProducto,
        producto.Imagen
    });
}

        // PUT: api/productos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductoRespuestaDto>>
            ActualizarProducto(
                int id,
                ActualizarProductoDto dto)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(producto =>
                    producto.IdProducto == id);

            if (producto is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el producto con ID {id}."
                });
            }

            var restauranteExiste = await _context.Restaurantes
                .AnyAsync(restaurante =>
                    restaurante.IdRestaurante == dto.IdRestaurante);

            if (!restauranteExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"El restaurante con ID {dto.IdRestaurante} no existe."
                });
            }

            var categoriaExiste = await _context.Categorias
                .AnyAsync(categoria =>
                    categoria.IdCategoria == dto.IdCategoria);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        $"La categoría con ID {dto.IdCategoria} no existe."
                });
            }

            var nombreRepetido = await _context.Productos
                .AnyAsync(otroProducto =>
                    otroProducto.IdProducto != id &&
                    otroProducto.IdRestaurante == dto.IdRestaurante &&
                    otroProducto.Nombre.ToLower() ==
                    dto.Nombre.Trim().ToLower());

            if (nombreRepetido)
            {
                return Conflict(new
                {
                    mensaje =
                        "Otro producto del restaurante ya utiliza ese nombre."
                });
            }

            producto.IdRestaurante = dto.IdRestaurante;
            producto.IdCategoria = dto.IdCategoria;
            producto.Nombre = dto.Nombre.Trim();
            producto.Descripcion =
                string.IsNullOrWhiteSpace(dto.Descripcion)
                    ? null
                    : dto.Descripcion.Trim();
            producto.Precio = dto.Precio;
            producto.Imagen =
                string.IsNullOrWhiteSpace(dto.Imagen)
                    ? null
                    : dto.Imagen.Trim();
            producto.Disponible = dto.Disponible;
            

            await _context.SaveChangesAsync();

            var productoActualizado =
                await ObtenerProductoDto(producto.IdProducto);

            return Ok(productoActualizado);
        }

        // PATCH: api/productos/5/disponibilidad
        [HttpPatch("{id:int}/disponibilidad")]
        public async Task<ActionResult> CambiarDisponibilidad(
            int id,
            CambiarDisponibilidadProductoDto dto)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(producto =>
                    producto.IdProducto == id);

            if (producto is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el producto con ID {id}."
                });
            }

            producto.Disponible = dto.Disponible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Disponibilidad actualizada correctamente.",
                producto.IdProducto,
                producto.Nombre,
                producto.Disponible
            });
        }

        // DELETE: api/productos/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(producto =>
                    producto.IdProducto == id);

            if (producto is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe el producto con ID {id}."
                });
            }

            var tieneDetallesPedido =
                await _context.DetallesPedido.AnyAsync(detalle =>
                    detalle.IdProducto == id);

            var tieneDetallesCarrito =
                await _context.DetallesCarrito.AnyAsync(detalle =>
                    detalle.IdProducto == id);

            if (tieneDetallesPedido || tieneDetallesCarrito)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar el producto porque está " +
                        "relacionado con pedidos o carritos. Cambia su " +
                        "disponibilidad a false en lugar de eliminarlo."
                });
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Producto eliminado correctamente."
            });
        }

        private async Task<ProductoRespuestaDto?>
            ObtenerProductoDto(int idProducto)
        {
            return await _context.Productos
                .AsNoTracking()
                .Where(producto =>
                    producto.IdProducto == idProducto)
                .Select(producto => new ProductoRespuestaDto
                {
                    IdProducto = producto.IdProducto,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre,
                    IdCategoria = producto.IdCategoria,
                    Categoria = producto.Categoria.Nombre,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,
                    
                })
                .FirstOrDefaultAsync();
        }
    }
}