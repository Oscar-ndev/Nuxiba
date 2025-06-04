using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuxibaApi.Data;
using NuxibaApi.Models;
using System.Globalization;
using System.Text;
using CsvHelper;

namespace NuxibaApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public LoginsController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Login>>> GetLogins()
        {
            var logins = await _appDbContext.Logins.Include(l => l.User).ToListAsync();

            return Ok(logins);
        }

        [HttpPost]
        public async Task<ActionResult<Login>> PostLogin(Login login)
        {
            var usuarioExistente = await _appDbContext.Users.AnyAsync(u => u.Id == login.User_id);
            if (!usuarioExistente)
            {
                return BadRequest($"El Usuario con Id {login.User_id} no existe");
            }
            if (login.Fecha == default)
            {
                return BadRequest("La Fecha proporcionada no es válida");
            }
            if (login.TipoMov == 1)
            {
                var ultimoMov = await _appDbContext.Logins.Where(l => l.User_id == login.User_id).OrderByDescending(l => l.Fecha)
                    .FirstOrDefaultAsync();

                if (ultimoMov != null && ultimoMov.TipoMov == 1)
                {
                    return BadRequest("No se puede iniciar sesión nuevamente, cierre la sesión anterior porfavor");
                }
            }else if(login.TipoMov == 0)
            {
                var ultimoMov = await _appDbContext.Logins.Where(l => l.User_id == login.User_id).OrderByDescending(l => l.Fecha)
                    .FirstOrDefaultAsync();

                if (ultimoMov == null || ultimoMov.TipoMov != 1)
                {
                    return BadRequest("No se puede cerrar la sesión sin haber iniciado sesión previamente porfavor");
                }
            }

            _appDbContext.Logins.Add(login);
            await _appDbContext.SaveChangesAsync();

            

            return CreatedAtAction(nameof(GetLogins), new {id = login.Id}, login);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogin(int id, Login login)
        {
            if (id != login.Id)
                return BadRequest("El ID del login no coincide con la ruta.");

            var loginOriginal = await _appDbContext.Logins.FindAsync(id);
            if (loginOriginal == null)
                return NotFound($"No existe un login con ID {id}");

            
            var userExists = await _appDbContext.Users.AnyAsync(u => u.Id == login.User_id);
            if (!userExists)
                return BadRequest($"El usuario con ID {login.User_id} no existe.");

            
            if (login.Fecha == default)
                return BadRequest("La fecha proporcionada no es válida.");

            
            var movimientos = await _appDbContext.Logins
                .Where(l => l.User_id == login.User_id && l.Id != id) 
                .OrderByDescending(l => l.Fecha)
                .ToListAsync();

            var ultimoMov = movimientos.FirstOrDefault();

            if (login.TipoMov == 1) // login
            {
                if (ultimoMov != null && ultimoMov.TipoMov == 1)
                    return BadRequest("No puedes cambiar este registro a login si ya hay uno anterior sin logout.");
            }
            else if (login.TipoMov == 0) // logout
            {
                if (ultimoMov == null || ultimoMov.TipoMov != 1)
                    return BadRequest("No puedes cambiar este registro a logout si no hay un login anterior.");
            }

            loginOriginal.User_id = login.User_id;
            loginOriginal.Extension = login.Extension;
            loginOriginal.TipoMov = login.TipoMov;
            loginOriginal.Fecha = login.Fecha;

            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogin(int id)
        {
            var login = await _appDbContext.Logins.FindAsync(id);

            if (login == null)
            {
                return NotFound($"No se encontró un registro con ID {id}.");
            }

            _appDbContext.Logins.Remove(login);
            await _appDbContext.SaveChangesAsync();

            return NoContent(); // 204
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetUserReportCsv()
        {
            var usuarios = await _appDbContext.Users
                .Include(u => u.Area)
                .ToListAsync();

            var report = usuarios.Select(u => new
            {
                Login = u.Login,
                NombreCompleto = $"{u.Nombres} {u.ApellidoPaterno} {u.ApellidoMaterno}",
                Area = u.Area?.Nombre ?? ""
            }).ToList();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(report);
            writer.Flush();

            var fileName = $"reporte_usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(memoryStream.ToArray(), "text/csv", fileName);
        }
    }
}
