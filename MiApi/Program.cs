using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiApi.Data;
using MiApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Obtener la cadena de conexión
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'.");

// Configurar Entity Framework Core con MySQL
builder.Services.AddDbContext<MyMDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );

    // Útil durante el desarrollo para mostrar errores de EF Core
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Servicio para generar hash de contraseñas
builder.Services.AddScoped<
    IPasswordHasher<Usuarios>,
    PasswordHasher<Usuarios>>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger solamente en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();