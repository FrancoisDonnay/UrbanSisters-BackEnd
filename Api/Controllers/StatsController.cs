using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    public class StatsController : ControllerBase
    {
        private readonly UrbanSisterContext _context;

        public StatsController(UrbanSisterContext context)
        {
            this._context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(Dto.Stats), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            List<User> users = await _context.User.Include(u => u.Relookeuse).ThenInclude(rel => rel.Appointment).ToListAsync();
            
            Dto.Stats stats = new Dto.Stats();
            stats.AdminCount = users.Count(u => u.IsAdmin);
            stats.ClientCount = users.Count;

            List<User> relookeuses = users.Where(u => u.Relookeuse != null).ToList();
            
            stats.RelookeuseCount = relookeuses.Count;

            List<User> relookeuseWithMark = relookeuses.Where(u => u.Relookeuse.AvgMark != null).ToList();
            stats.AvgRelookeuseMark = relookeuseWithMark.Aggregate(0.0, (d, user) => d + user.Relookeuse.AvgMark.Value) / relookeuseWithMark.Count;

            stats.AvgNumberAppointment = relookeuses.Aggregate(0.0, (i, user) => i + user.Relookeuse.Appointment.Count) / relookeuses.Count;
            
            return Ok(stats);
        }
    }
}