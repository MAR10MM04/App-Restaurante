using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiApi.Models;

namespace MiApi.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(MyMDbContext context, IPasswordHasher<Usuarios> passwordHasher)
        {
            // Esperar a que la BD esté creada y al día con migraciones
            await context.Database.MigrateAsync();

            // Estos catálogos deben existir aunque la base ya tenga usuarios.
            var rolesRequeridos = new[]
            {
                new Rol { Nombre = "Administrador", Descripcion = "Administración de la plataforma" },
                new Rol { Nombre = "Cliente", Descripcion = "Cliente de la aplicación" },
                new Rol { Nombre = "Propietario", Descripcion = "Propietario de restaurante" },
                new Rol { Nombre = "Repartidor", Descripcion = "Repartidor de pedidos" }
            };

            var nombresRolesExistentes = await context.Roles
                .Select(rol => rol.Nombre.ToLower())
                .ToListAsync();

            context.Roles.AddRange(rolesRequeridos.Where(rol =>
                !nombresRolesExistentes.Contains(rol.Nombre.ToLower())));
            await context.SaveChangesAsync();

            if (!await context.Usuarios.AnyAsync())
            {
                var admin = new Usuarios
                {
                    Nombre = "Admin Local",
                    Email = "admin@app.com",
                    Telefono = "1234567890",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo"
                };
                admin.ContrasenaHash = passwordHasher.HashPassword(admin, "Admin123!");

                var cliente = new Usuarios
                {
                    Nombre = "Cliente Prueba",
                    Email = "cliente@app.com",
                    Telefono = "0987654321",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo"
                };
                cliente.ContrasenaHash = passwordHasher.HashPassword(cliente, "Cliente123!");

                var repartidor = new Usuarios
                {
                    Nombre = "Repartidor Juan",
                    Email = "juan@reparto.com",
                    Telefono = "1112223334",
                    FechaRegistro = DateTime.UtcNow,
                    Estado = "Activo"
                };
                repartidor.ContrasenaHash = passwordHasher.HashPassword(repartidor, "Repartidor123!");

                context.Usuarios.AddRange(admin, cliente, repartidor);
                await context.SaveChangesAsync();

                // Crear Perfil de Repartidor
                context.Repartidores.Add(new Repartidor
                {
                    IdUsuario = repartidor.IdUsuario,
                    Estado = "Activo"
                });
                await context.SaveChangesAsync();

                // Categorias
                var catPizza = new Categoria { Nombre = "Pizza", Descripcion = "Pizzas de todo tipo" };
                var catSushi = new Categoria { Nombre = "Sushi", Descripcion = "Comida Japonesa" };
                var catBurger = new Categoria { Nombre = "Hamburguesas", Descripcion = "Hamburguesas y comida rapida" };
                var catPostres = new Categoria { Nombre = "Postres", Descripcion = "Dulces y postres" };
                var catBebidas = new Categoria { Nombre = "Bebidas", Descripcion = "Bebidas frias y calientes" };

                context.Categorias.AddRange(catPizza, catSushi, catBurger, catPostres, catBebidas);
                await context.SaveChangesAsync();

                // Restaurantes
                var rest1 = new Restaurante
                {
                    IdUsuarioPropietario = admin.IdUsuario,
                    Nombre = "Pizzeria Napoli Centrale",
                    Descripcion = "La mejor pizza de la ciudad",
                    Direccion = "Av. Siempre Viva 123",
                    Telefono = "555-0001",
                    HorarioApertura = "10:00",
                    HorarioCierre = "22:00",
                    Imagen = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=900&q=85",
                    Latitud = 19.4326,
                    Longitud = -99.1332
                };

                var rest2 = new Restaurante
                {
                    IdUsuarioPropietario = admin.IdUsuario,
                    Nombre = "Sushi Zen Master",
                    Descripcion = "Autentico sushi japones",
                    Direccion = "Calle Luna 456",
                    Telefono = "555-0002",
                    HorarioApertura = "12:00",
                    HorarioCierre = "23:00",
                    Imagen = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=900&q=85",
                    Latitud = 19.4284,
                    Longitud = -99.1601
                };

                var rest3 = new Restaurante
                {
                    IdUsuarioPropietario = admin.IdUsuario,
                    Nombre = "Burger King",
                    Descripcion = "Hamburguesas clasicas",
                    Direccion = "Blvd. Sol 789",
                    Telefono = "555-0003",
                    HorarioApertura = "09:00",
                    HorarioCierre = "21:00",
                    Imagen = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=1200&q=85",
                    Latitud = 19.4300,
                    Longitud = -99.1500
                };

                context.Restaurantes.AddRange(rest1, rest2, rest3);
                await context.SaveChangesAsync();

                // Productos
                context.Productos.AddRange(
                    new Producto { IdRestaurante = rest1.IdRestaurante, IdCategoria = catPizza.IdCategoria, Nombre = "Pizza Pepperoni", Descripcion = "Deliciosa pizza de pepperoni con queso", Precio = 150.00m, Imagen = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=800&q=80", Disponible = true },
                    new Producto { IdRestaurante = rest1.IdRestaurante, IdCategoria = catPizza.IdCategoria, Nombre = "Pizza Hawaiana", Descripcion = "Pizza con piña y jamon", Precio = 140.00m, Imagen = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80", Disponible = true },
                    
                    new Producto { IdRestaurante = rest2.IdRestaurante, IdCategoria = catSushi.IdCategoria, Nombre = "Dragon Roll", Descripcion = "Rollo de sushi con aguacate y anguila", Precio = 180.00m, Imagen = "https://images.unsplash.com/photo-1553621042-f6e147245754?auto=format&fit=crop&w=800&q=80", Disponible = true },
                    new Producto { IdRestaurante = rest2.IdRestaurante, IdCategoria = catSushi.IdCategoria, Nombre = "Nigiri Salmon", Descripcion = "2 piezas de nigiri de salmon", Precio = 90.00m, Imagen = "https://images.unsplash.com/photo-1611143669185-af224c5e3252?auto=format&fit=crop&w=800&q=80", Disponible = true },
                    
                    new Producto { IdRestaurante = rest3.IdRestaurante, IdCategoria = catBurger.IdCategoria, Nombre = "Hamburguesa Clasica", Descripcion = "Carne, queso, lechuga y tomate", Precio = 100.00m, Imagen = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80", Disponible = true },
                    new Producto { IdRestaurante = rest3.IdRestaurante, IdCategoria = catBebidas.IdCategoria, Nombre = "Refresco Cola", Descripcion = "600ml", Precio = 30.00m, Imagen = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?auto=format&fit=crop&w=800&q=80", Disponible = true }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
