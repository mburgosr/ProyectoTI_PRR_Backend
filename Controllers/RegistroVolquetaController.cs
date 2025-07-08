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
        var registro = await _context.RegistroVolquetas.FindAsync(id);
        if (registro == null) return NotFound();

        _context.RegistroVolquetas.Remove(registro);
        await _context.SaveChangesAsync();

        return Ok();
    }
} }


