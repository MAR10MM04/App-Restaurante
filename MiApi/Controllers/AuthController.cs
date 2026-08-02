using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiApi.Data;
using MiApi.DTOs;
using MiApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyMDbContext _context;
        private readonly IPasswordHasher<Usuarios> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthController(
            MyMDbContext context,
            IPasswordHasher<Usuarios> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        // POST: api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginRespuestaDto>> Login(
            [FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email
                .Trim()
                .ToLowerInvariant();

            var usuario = await _context.Usuarios
                .Include(usuario => usuario.UsuariosRoles)
                    .ThenInclude(usuarioRol => usuarioRol.Rol)
                .FirstOrDefaultAsync(usuario =>
                    usuario.Email.ToLower() == email);

            if (usuario is null)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            if (!usuario.Estado.Equals(
                    "Activo",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    mensaje = "El usuario no se encuentra activo."
                });
            }

            var resultado =
                _passwordHasher.VerifyHashedPassword(
                    usuario,
                    usuario.ContrasenaHash,
                    dto.Contrasena);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            var roles = usuario.UsuariosRoles
                .Select(usuarioRol => usuarioRol.Rol.Nombre)
                .Distinct()
                .ToList();

            var token = GenerarToken(usuario, roles);

            return Ok(new LoginRespuestaDto
            {
                Token = token,
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Roles = roles
            });
        }

        private string GenerarToken(
            Usuarios usuario,
            IEnumerable<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Key.");

            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Issuer.");

            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Audience.");

            var duracionTexto =
                _configuration["Jwt:DurationInMinutes"];

            var duracionMinutos =
                int.TryParse(duracionTexto, out var minutos)
                    ? minutos
                    : 60;

            if (duracionMinutos <= 0)
            {
                duracionMinutos = 60;
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new(
                    ClaimTypes.Name,
                    usuario.Nombre),

                new(
                    ClaimTypes.Email,
                    usuario.Email),

                new(
                    JwtRegisteredClaimNames.Sub,
                    usuario.IdUsuario.ToString()),

                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            foreach (var rol in roles)
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    rol));
            }

            var clave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credenciales = new SigningCredentials(
                clave,
                SecurityAlgorithms.HmacSha256);

            var fechaExpiracion = DateTime.UtcNow
                .AddMinutes(duracionMinutos);

            var jwtToken = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: fechaExpiracion,
                signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler()
                .WriteToken(jwtToken);

            return token;
        }
    }
}