using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly MyMDbContext _context;

        public CategoriasController(MyMDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias
        // GET: api/categorias?buscar=bebidas
        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<CategoriaRespuestaDto>>>
            ObtenerCategorias([FromQuery] string? buscar)
        {
            var consulta = _context.Categorias
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();

                consulta = consulta.Where(categoria =>
                    categoria.Nombre.Contains(texto) ||
                    categoria.Descripcion.Contains(texto));
            }

            var categorias = await consulta
                .OrderBy(categoria => categoria.Nombre)
                .Select(categoria => new CategoriaRespuestaDto
                {
                    IdCategoria = categoria.IdCategoria,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    CantidadProductos = categoria.Productos.Count()
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/categorias/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaRespuestaDto>>
            ObtenerCategoriaPorId(int id)
        {
            var categoria = await ObtenerCategoriaDto(id);

            if (categoria is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la categoría con ID {id}."
                });
            }

            return Ok(categoria);
        }

        // GET: api/categorias/5/detalle
        [HttpGet("{id:int}/detalle")]
        public async Task<ActionResult<CategoriaDetalleDto>>
            ObtenerCategoriaConProductos(int id)
        {
            var categoria = await _context.Categorias
                .AsNoTracking()
                .Where(categoria =>
                    categoria.IdCategoria == id)
                .Select(categoria => new CategoriaDetalleDto
                {
                    IdCategoria = categoria.IdCategoria,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,

                    CantidadProductos =
                        categoria.Productos.Count(),

                    Productos = categoria.Productos
                        .OrderBy(producto => producto.Nombre)
                        .Select(producto =>
                            new ProductoCategoriaDto
                            {
                                IdProducto =
                                    producto.IdProducto,

                                Nombre =
                                    producto.Nombre,

                                Descripcion =
                                    producto.Descripcion,

                                Precio =
                                    producto.Precio,

                                Imagen =
                                    producto.Imagen,

                                Disponible =
                                    producto.Disponible,

                                IdRestaurante =
                                    producto.IdRestaurante,

                                Restaurante =
                                    producto.Restaurante.Nombre
                            })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (categoria is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la categoría con ID {id}."
                });
            }

            return Ok(categoria);
        }

        // GET: api/categorias/5/productos
        [HttpGet("{id:int}/productos")]
        public async Task<ActionResult<IEnumerable<ProductoCategoriaDto>>>
            ObtenerProductosPorCategoria(
                int id,
                [FromQuery] bool? disponible)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(categoria =>
                    categoria.IdCategoria == id);

            if (!categoriaExiste)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la categoría con ID {id}."
                });
            }

            var consulta = _context.Productos
                .AsNoTracking()
                .Where(producto =>
                    producto.IdCategoria == id);

            if (disponible.HasValue)
            {
                consulta = consulta.Where(producto =>
                    producto.Disponible == disponible.Value);
            }

            var productos = await consulta
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new ProductoCategoriaDto
                {
                    IdProducto = producto.IdProducto,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Imagen = producto.Imagen,
                    Disponible = producto.Disponible,
                    IdRestaurante = producto.IdRestaurante,
                    Restaurante = producto.Restaurante.Nombre
                })
                .ToListAsync();

            return Ok(productos);
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<CategoriaRespuestaDto>>
            CrearCategoria(CrearCategoriaDto dto)
        {
            var nombre = dto.Nombre.Trim();
            

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la categoría es obligatorio."
                });
            }

            var categoriaExistente = await _context.Categorias
                .AnyAsync(categoria =>
                    categoria.Nombre.ToLower() ==
                    nombre.ToLower());

            if (categoriaExistente)
            {
                return Conflict(new
                {
                    mensaje =
                        "Ya existe una categoría con ese nombre."
                });
            }

            var nuevaCategoria = new Categoria
            {
                Nombre = nombre,
                
            };

            _context.Categorias.Add(nuevaCategoria);
            await _context.SaveChangesAsync();

            var categoriaCreada = await ObtenerCategoriaDto(
                nuevaCategoria.IdCategoria);

            return CreatedAtAction(
                nameof(ObtenerCategoriaPorId),
                new { id = nuevaCategoria.IdCategoria },
                categoriaCreada
            );
        }

        // PUT: api/categorias/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaRespuestaDto>>
            ActualizarCategoria(
                int id,
                ActualizarCategoriaDto dto)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(categoria =>
                    categoria.IdCategoria == id);

            if (categoria is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la categoría con ID {id}."
                });
            }

            var nombre = dto.Nombre.Trim();
            var descripcion = dto.Descripcion.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new
                {
                    mensaje = "El nombre de la categoría es obligatorio."
                });
            }

            var nombreDuplicado = await _context.Categorias
                .AnyAsync(otraCategoria =>
                    otraCategoria.IdCategoria != id &&
                    otraCategoria.Nombre.ToLower() ==
                    nombre.ToLower());

            if (nombreDuplicado)
            {
                return Conflict(new
                {
                    mensaje =
                        "Ya existe otra categoría con ese nombre."
                });
            }

            categoria.Nombre = nombre;
            categoria.Descripcion = descripcion;

            await _context.SaveChangesAsync();

            return Ok(await ObtenerCategoriaDto(id));
        }

        // DELETE: api/categorias/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(categoria =>
                    categoria.IdCategoria == id);

            if (categoria is null)
            {
                return NotFound(new
                {
                    mensaje = $"No existe la categoría con ID {id}."
                });
            }

            var tieneProductos = await _context.Productos
                .AnyAsync(producto =>
                    producto.IdCategoria == id);

            if (tieneProductos)
            {
                return Conflict(new
                {
                    mensaje =
                        "No se puede eliminar la categoría porque " +
                        "tiene productos relacionados."
                });
            }

            _context.Categorias.Remove(categoria);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje =
                        "No fue posible eliminar la categoría porque " +
                        "tiene registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje = "Categoría eliminada correctamente."
            });
        }

        private async Task<CategoriaRespuestaDto?>
            ObtenerCategoriaDto(int idCategoria)
        {
            return await _context.Categorias
                .AsNoTracking()
                .Where(categoria =>
                    categoria.IdCategoria == idCategoria)
                .Select(categoria => new CategoriaRespuestaDto
                {
                    IdCategoria = categoria.IdCategoria,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    CantidadProductos =
                        categoria.Productos.Count()
                })
                .FirstOrDefaultAsync();
        }
    }
}