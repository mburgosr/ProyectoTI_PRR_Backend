using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.DTOs;
using ProyectoTI_PRR_Backend.Models;

namespace ProyectoTI_PRR_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == login.NombreUsuario);
            if (user == null)
                return Unauthorized("Usuario no encontrado");

            bool passwordValida = BCrypt.Net.BCrypt.Verify(login.Contraseña, user.ContraseñaHash);
            if (!passwordValida)
                return Unauthorized("Contraseña incorrecta");

            return Ok(new { mensaje = "Login exitoso", usuario = user.NombreUsuario });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registro)
        {
            if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == registro.NombreUsuario))
                return BadRequest("Usuario ya existe");

            var usuario = new Usuario
            {
                NombreUsuario = registro.NombreUsuario,
                ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(registro.Contraseña),
                CorreoElectronico = registro.CorreoElectronico
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado");
        }
    }
}
