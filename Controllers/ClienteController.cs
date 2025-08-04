using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

[Route("api/[controller]")]
[ApiController]
public class ClienteController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClienteController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Cliente
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
    {
        return await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
    }

    // GET: api/Cliente/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetCliente(string id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound();  // Si no se encuentra, devuelve un error 404
        }
        return cliente;
    }

    // POST: api/Cliente
    [HttpPost]
    public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
    {
        var existingCliente = await _context.Clientes.FindAsync(cliente.Cedula);
        if (existingCliente != null)
        {
            return Conflict("El cliente con esta cédula ya existe.");
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        // Devuelve la respuesta con el nuevo cliente y un código 201 (creado)
        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Cedula }, cliente);
    }

    // PUT: api/Cliente/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCliente(string id, Cliente cliente)
    {
        if (id != cliente.Cedula)
        {
            return BadRequest("La cédula del cliente no coincide con la proporcionada en la URL.");
        }

        _context.Entry(cliente).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Clientes.Any(c => c.Cedula == id))
            {
                return NotFound();  // Si el cliente no se encuentra en la base de datos
            }
            else
            {
                throw;
            }
        }

        return NoContent();  // Respuesta 204 (sin contenido) indica que la actualización fue exitosa
    }

    // DELETE: api/Cliente/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCliente(string id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound();  // Si el cliente no se encuentra, devuelve un error 404
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();  // Respuesta 204 (sin contenido) indica que el cliente fue eliminado con éxito
    }

    
}