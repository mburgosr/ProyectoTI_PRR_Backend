using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

    [Route("api/[controller]")]
    [ApiController]
    public class MaterialesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Materiales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Materiales>>> GetMateriales()
        {
            return await _context.Materiales.ToListAsync() ;
        }

        // GET api/Materiales/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Materiales>> GetMateriales(string id)
        {
            var materiales = await _context.Materiales.FindAsync(id);
            if (materiales == null)
            {
                return NotFound();  // Si no se encuentra, devuelve un error 404
            }
            return materiales;
        }

        // POST api/<ValuesController>
        [HttpPost]
        public async Task<ActionResult<Materiales>> PostMateriales(Materiales materiales)
        {
            var existingMateriales = await _context.Materiales.FindAsync(materiales.Codigo);
            if (existingMateriales != null)
            {
                return Conflict("El material con este código ya existe.");
            }

            _context.Materiales.Add(materiales);
            await _context.SaveChangesAsync();

            // Devuelve la respuesta con el nuevo material y un código 201 (creado)
            return CreatedAtAction(nameof(GetMateriales), new { id = materiales.Codigo }, materiales);
        }

        // PUT api/Materiales/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMateriales(string id, Materiales materiales)
        {
            if (id != materiales.Codigo)
            {
                return BadRequest("El código del material no coincide con la proporcionada en la URL.");
            }

            _context.Entry(materiales).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Materiales.Any(c => c.Codigo == id))
                {
                    return NotFound();  // Si el material no se encuentra en la base de datos
                }
                else
                {
                    throw;
                }
            }

            return NoContent();  // Respuesta 204 (sin contenido) indica que la actualización fue exitosa
        }

        // DELETE api/Materiales/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMateriales(string id)
        {
            var materiales = await _context.Materiales.FindAsync(id);
            if (materiales == null)
            {
                return NotFound();  // Si el material no se encuentra, devuelve un error 404
            }

            _context.Materiales.Remove(materiales);
            await _context.SaveChangesAsync();

            return NoContent();  // Respuesta 204 (sin contenido) indica que el material fue eliminado con éxito
        }
    }
