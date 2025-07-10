using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;
namespace ProyectoTI_PRR_Backend.Controllers
{[ApiController]
[Route("api/[controller]")]
public class RegistroVolquetaController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RegistroVolquetaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var registros = await _context.RegistroVolquetas.ToListAsync();
        return Ok(registros);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RegistroVolqueta registro)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.RegistroVolquetas.Add(registro);
        await _context.SaveChangesAsync();
        return Ok(registro);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] RegistroVolqueta registro)
    {
        if (id != registro.Id)
            return BadRequest();

        _context.Entry(registro).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var registro = await _context.RegistroVolquetas.FindAsync(id);
                if (registro == null)
                    return NotFound();

                // Validar si la volqueta está asociada a algún reporte diario
                bool tieneReportes = await _context.ReportesDiarios.AnyAsync(r => r.VolquetaId == id);
                if (tieneReportes)
                {
                    // Mensaje para mostrar en el frontend
                    return BadRequest("No se puede eliminar esta volqueta porque está asociada a reportes diarios.");
                }

                _context.RegistroVolquetas.Remove(registro);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // Mostrar en consola para depuración
                Console.WriteLine("Error en Delete: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                // Respuesta controlada para el frontend
                return BadRequest("Error al eliminar la volqueta: " + ex.Message);
            }
        }



    }
}


