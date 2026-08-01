using Microsoft.EntityFrameworkCore;
using MiApi.Models;

namespace MiApi.Data
{
    public class MyMDbContext : DbContext
    {
        public MyMDbContext(
            DbContextOptions<MyMDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuarios> Usuarios => Set<Usuarios>();

        public DbSet<Restaurante> Restaurantes =>
            Set<Restaurante>();

        public DbSet<Categoria> Categorias =>
            Set<Categoria>();

        public DbSet<Producto> Productos =>
            Set<Producto>();



        public DbSet<Repartidor> Repartidores =>
            Set<Repartidor>();

        public DbSet<Pedido> Pedidos =>
            Set<Pedido>();

        public DbSet<DetallePedido> DetallesPedido =>
            Set<DetallePedido>();

        public DbSet<Pago> Pagos =>
            Set<Pago>();

        public DbSet<Carritos> Carritos =>
            Set<Carritos>();

        public DbSet<DetalleCarrito> DetallesCarrito =>
            Set<DetalleCarrito>();
         public DbSet<Direccion> Direcciones =>
            Set<Direccion>();
        public DbSet<Rol> Roles { get; set; }

         public DbSet<UsuarioRol> UsuariosRoles { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarUsuario(modelBuilder);
            ConfigurarRestaurante(modelBuilder);
            ConfigurarCategoria(modelBuilder);
            ConfigurarProducto(modelBuilder);
            ConfigurarRepartidor(modelBuilder);
            ConfigurarPedido(modelBuilder);
            ConfigurarDetallePedido(modelBuilder);
            ConfigurarPago(modelBuilder);
            ConfigurarCarrito(modelBuilder);
            ConfigurarDireccion(modelBuilder);
            ConfigurarDetalleCarrito(modelBuilder);
            ConfigurarRol(modelBuilder);
            ConfigurarUsuarioRol(modelBuilder);
        }

        private static void ConfigurarUsuario(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuarios>(entity =>
            {
                entity.ToTable("usuarios");

                entity.HasKey(u => u.IdUsuario);

                entity.Property(u => u.IdUsuario)
                    .HasColumnName("id_usuario")
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.Nombre)
                    .HasColumnName("nombre")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("email")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(20);

                entity.Property(u => u.ContrasenaHash)
                    .HasColumnName("contrasena_hash")
                    .HasMaxLength(255)
                    .IsRequired();

entity.Property(u => u.FechaRegistro)
    .HasColumnName("fecha_registro")
    .HasColumnType("timestamp")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");



                entity.Property(u => u.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(20)
                    .HasDefaultValue("Activo");


                // Usuario 1:N Restaurantes
                entity.HasMany(u => u.Restaurantes)
                    .WithOne(r => r.UsuarioPropietario)
                    .HasForeignKey(r => r.IdUsuarioPropietario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Usuario 1:N Pedidos
                entity.HasMany(u => u.Pedidos)
                    .WithOne(p => p.Usuario)
                    .HasForeignKey(p => p.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Usuario 1:N Direcciones
                entity.HasMany(u => u.Direcciones)
                    .WithOne(d => d.Usuario)
                    .HasForeignKey(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);

                // Usuario 1:N Carritos
                entity.HasMany(u => u.Carritos)
                    .WithOne(c => c.Usuario)
                    .HasForeignKey(c => c.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        private static void ConfigurarUsuarioRol(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UsuarioRol>(entity =>
    {
        entity.ToTable("usuarios_roles");

        entity.HasKey(x => new
        {
            x.IdUsuario,
            x.IdRol
        });

entity.Property(x => x.FechaAsignacion)
    .HasColumnName("fecha_asignacion")
    .HasColumnType("timestamp")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuariosRoles)
            .HasForeignKey(x => x.IdUsuario);

        entity.HasOne(x => x.Rol)
            .WithMany(x => x.UsuariosRoles)
            .HasForeignKey(x => x.IdRol);
    });
}
private static void ConfigurarRol(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Rol>(entity =>
    {
        entity.ToTable("roles");

        entity.HasKey(x => x.IdRol);

        entity.Property(x => x.Nombre)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.Descripcion)
            .HasMaxLength(200);

        entity.HasIndex(x => x.Nombre)
            .IsUnique();
    });
}
        private static void ConfigurarRestaurante(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Restaurante>(entity =>
            {
                entity.ToTable("restaurantes");

                entity.HasKey(r => r.IdRestaurante);

                entity.Property(r => r.IdRestaurante)
                    .HasColumnName("id_restaurante")
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.IdUsuarioPropietario)
                    .HasColumnName("id_usuario_propietario");

                entity.Property(r => r.Nombre)
                    .HasColumnName("nombre")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(r => r.Direccion)
                    .HasColumnName("direccion")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(r => r.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(20);

                entity.Property(r => r.HorarioApertura)
                    .HasColumnName("horario_apertura");

                entity.Property(r => r.HorarioCierre)
                    .HasColumnName("horario_cierre");


                entity.Property(r => r.Latitud)
                    .HasColumnName("latitud")
                    .HasPrecision(10, 7);

                entity.Property(r => r.Longitud)
                    .HasColumnName("longitud")
                    .HasPrecision(10, 7);

                // Restaurante 1:N Productos
                entity.HasMany(r => r.Productos)
                    .WithOne(p => p.Restaurante)
                    .HasForeignKey(p => p.IdRestaurante)
                    .OnDelete(DeleteBehavior.Cascade);

                // Restaurante 1:N Pedidos
                entity.HasMany(r => r.Pedidos)
                    .WithOne(p => p.Restaurante)
                    .HasForeignKey(p => p.IdRestaurante)
                    .OnDelete(DeleteBehavior.Restrict);

                // Restaurante 1:N Carritos
                entity.HasMany(r => r.Carritos)
                    .WithOne(c => c.Restaurante)
                    .HasForeignKey(c => c.IdRestaurante)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
                private static void ConfigurarDireccion(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.ToTable("direcciones");

                entity.HasKey(d => d.IdDireccion);

                entity.Property(d => d.IdDireccion)
                    .HasColumnName("id_direccion")
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.IdUsuario)
                    .HasColumnName("id_usuario");



                entity.Property(d => d.Latitud)
                    .HasColumnName("latitud")
                    .HasPrecision(10, 7);

                entity.Property(d => d.Longitud)
                    .HasColumnName("longitud")
                    .HasPrecision(10, 7);
            });
        }

        private static void ConfigurarCategoria(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("categorias");

                entity.HasKey(c => c.IdCategoria);

                entity.Property(c => c.IdCategoria)
                    .HasColumnName("id_categoria")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.Nombre)
                    .HasColumnName("nombre")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(c => c.Descripcion)
                    .HasColumnName("descripcion")
                    .HasMaxLength(255);

                entity.HasIndex(c => c.Nombre)
                    .IsUnique();
            });
        }

        private static void ConfigurarProducto(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");

                entity.HasKey(p => p.IdProducto);

                entity.Property(p => p.IdProducto)
                    .HasColumnName("id_producto")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.IdRestaurante)
                    .HasColumnName("id_restaurante");

                entity.Property(p => p.IdCategoria)
                    .HasColumnName("id_categoria");

                entity.Property(p => p.Nombre)
                    .HasColumnName("nombre")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(p => p.Descripcion)
                    .HasColumnName("descripcion")
                    .HasMaxLength(500);

                entity.Property(p => p.Precio)
                    .HasColumnName("precio")
                    .HasPrecision(10, 2);

                entity.Property(p => p.Disponible)
                    .HasColumnName("disponible")
                    .HasDefaultValue(true);

               

                // Producto N:1 Categoría
                entity.HasOne(p => p.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(p => p.IdCategoria)
                    .OnDelete(DeleteBehavior.Restrict);

                // La relación con Restaurante se configura
                // también desde Restaurante.
            });
        }

     

        private static void ConfigurarRepartidor(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Repartidor>(entity =>
            {
                entity.ToTable("repartidores");

                entity.HasKey(r => r.IdRepartidor);

                entity.Property(r => r.IdRepartidor)
                    .HasColumnName("id_repartidor")
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(r => r.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(20)
                    .HasDefaultValue("Activo");


                // Impide que un usuario tenga dos perfiles
                // de repartidor.
                entity.HasIndex(r => r.IdUsuario)
                    .IsUnique();

                // Usuario 1:0..1 Repartidor
                entity.HasOne(r => r.Usuario)
                    .WithOne(u => u.Repartidor)
                    .HasForeignKey<Repartidor>(
                        r => r.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigurarPedido(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("pedidos");

                entity.HasKey(p => p.IdPedido);

                entity.Property(p => p.IdPedido)
                    .HasColumnName("id_pedido")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(p => p.IdRestaurante)
                    .HasColumnName("id_restaurante");

                entity.Property(p => p.IdDireccionEntrega)
                    .HasColumnName("id_direccion_entrega");

                entity.Property(p => p.IdRepartidor)
                    .HasColumnName("id_repartidor");

                entity.Property(p => p.NumeroPedido)
                    .HasColumnName("numero_pedido")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(p => p.NumeroPedido)
                    .IsUnique();

entity.Property(p => p.FechaPedido)
    .HasColumnName("fecha_pedido")
    .HasColumnType("timestamp")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(p => p.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(p => p.Total)
                    .HasColumnName("total")
                    .HasPrecision(12, 2);

                entity.Property(p => p.TipoPago)
                    .HasColumnName("tipo_pago")
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(p => p.CalificacionRestaurante)
                    .HasColumnName("calificacion_restaurante");

                entity.Property(p => p.CalificacionRepartidor)
                    .HasColumnName("calificacion_repartidor");

                // Dirección 1:N Pedidos
                entity.HasOne(p => p.DireccionEntrega)
                    .WithMany(d => d.Pedidos)
                    .HasForeignKey(p => p.IdDireccionEntrega)
                    .OnDelete(DeleteBehavior.Restrict);

                // Repartidor 1:N Pedidos
                entity.HasOne(p => p.Repartidor)
                    .WithMany(r => r.Pedidos)
                    .HasForeignKey(p => p.IdRepartidor)
                    .OnDelete(DeleteBehavior.SetNull);

                // Pedido 1:N Detalles
                entity.HasMany(p => p.Detalles)
                    .WithOne(d => d.Pedido)
                    .HasForeignKey(d => d.IdPedido)
                    .OnDelete(DeleteBehavior.Cascade);

                // Pedido 1:N Pagos
         entity.HasOne(p => p.Pago)
      .WithOne(pg => pg.Pedido)
      .HasForeignKey<Pago>(pg => pg.IdPedido)
      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigurarDetallePedido(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.ToTable("detalle_pedido");

                entity.HasKey(d => d.IdDetallePedido);

                entity.Property(d => d.IdDetallePedido)
                    .HasColumnName("id_detalle_pedido")
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.IdPedido)
                    .HasColumnName("id_pedido");

                entity.Property(d => d.IdProducto)
                    .HasColumnName("id_producto");

                entity.Property(d => d.Cantidad)
                    .HasColumnName("cantidad");

                entity.Property(d => d.PrecioUnitario)
                    .HasColumnName("precio_unitario")
                    .HasPrecision(10, 2);

                entity.Property(d => d.Subtotal)
                    .HasColumnName("subtotal")
                    .HasPrecision(12, 2);



                // Producto 1:N DetallesPedido
                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetallesPedido)
                    .HasForeignKey(d => d.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.IdPedido);
                entity.HasIndex(d => d.IdProducto);
            });
        }

        private static void ConfigurarPago(
    ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Pago>(entity =>
    {
        entity.ToTable("pagos");

        entity.HasKey(p => p.IdPago);

        entity.Property(p => p.IdPago)
            .HasColumnName("id_pago")
            .ValueGeneratedOnAdd();

        entity.Property(p => p.IdPedido)
            .HasColumnName("id_pedido")
            .IsRequired();

        entity.Property(p => p.Monto)
            .HasColumnName("monto")
            .HasPrecision(12, 2)
            .IsRequired();

        entity.Property(p => p.MetodoPago)
            .HasColumnName("metodo_pago")
            .HasMaxLength(30)
            .IsRequired();

entity.Property(p => p.FechaPago)
    .HasColumnName("fecha_pago")
    .HasColumnType("timestamp")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasIndex(p => p.IdPedido)
            .IsUnique();
    });
}

        private static void ConfigurarCarrito(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Carritos>(entity =>
            {
                entity.ToTable("carritos");

                entity.HasKey(c => c.IdCarrito);

                entity.Property(c => c.IdCarrito)
                    .HasColumnName("id_carrito")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.IdUsuario)
                    .HasColumnName("id_usuario");

                entity.Property(c => c.IdRestaurante)
                    .HasColumnName("id_restaurante");

entity.Property(c => c.FechaCreacion)
    .HasColumnName("fecha_creacion")
    .HasColumnType("timestamp")
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(c => c.Estado)
                    .HasColumnName("estado")
                    .HasMaxLength(20)
                    .HasDefaultValue("Activo");

                // Carrito 1:N Detalles
                entity.HasMany(c => c.Detalles)
                    .WithOne(d => d.Carrito)
                    .HasForeignKey(d => d.IdCarrito)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(c => c.IdUsuario);
                entity.HasIndex(c => c.IdRestaurante);
            });
        }

        private static void ConfigurarDetalleCarrito(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalleCarrito>(entity =>
            {
                entity.ToTable("detalle_carrito");

                entity.HasKey(d => d.IdDetalleCarrito);

                entity.Property(d => d.IdDetalleCarrito)
                    .HasColumnName("id_detalle_carrito")
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.IdCarrito)
                    .HasColumnName("id_carrito");

                entity.Property(d => d.IdProducto)
                    .HasColumnName("id_producto");

                entity.Property(d => d.Cantidad)
                    .HasColumnName("cantidad");

                entity.Property(d => d.PrecioUnitario)
                    .HasColumnName("precio_unitario")
                    .HasPrecision(10, 2);

            

                // Producto 1:N DetallesCarrito
                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetallesCarrito)
                    .HasForeignKey(d => d.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.IdCarrito);
                entity.HasIndex(d => d.IdProducto);

                /*
                 * No se establece como único:
                 *
                 * entity.HasIndex(d => new
                 * {
                 *     d.IdCarrito,
                 *     d.IdProducto
                 * }).IsUnique();
                 *
                 * Un producto puede agregarse varias veces con
                 * opciones diferentes.
                 */
            });
        }
    }
}