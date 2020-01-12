using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "relookeuse")]
    public class TarifController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public TarifController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Dto.Tarif>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyTarifs()
        {
            IEnumerable<Tarif> tarifs = await _context.Tarif.Where(tar => tar.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).ToListAsync();
            return Ok(tarifs.Select(tar => _mapper.Map<Dto.Tarif>(tar)));
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Dto.Tarif), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddTarif([FromBody] Dto.NewTarif newTarif)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (await _context.Tarif.FirstOrDefaultAsync(t => t.RelookeuseId == userId && t.Service.ToLower().Equals(newTarif.Service.ToLower())) != null)
            {
                return Conflict();
            }
            
            Tarif tarif = _mapper.Map<Tarif>(newTarif);
            tarif.RelookeuseId = userId;
            var result = await _context.AddAsync(tarif);
            await _context.SaveChangesAsync();
            
            return Created("api/tarif/", _mapper.Map<Dto.Tarif>(result.Entity));
        }
        
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Dto.Tarif), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] Dto.Tarif updatedTarif)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Tarif tarif = await _context.Tarif.FirstOrDefaultAsync(t => t.Service.ToLower().Equals(updatedTarif.Service.ToLower()) && t.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            if (tarif == null)
            {
                return NotFound(updatedTarif.Service);
            }

            tarif.Price = updatedTarif.Price;
            _context.Entry(tarif).OriginalValues["RowVersion"] = updatedTarif.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }
            
            return Ok(_mapper.Map<Dto.Tarif>(tarif));
        }
        
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteTarif([FromBody] Dto.DeleteTarif delTarif)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Tarif tarif = await _context.Tarif.FirstOrDefaultAsync(t => t.Service.ToLower().Equals(delTarif.Service.ToLower()) && t.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            
            if (tarif == null)
            {
                return NotFound(delTarif.Service);
            }
            
            _context.Entry(tarif).OriginalValues["RowVersion"] = delTarif.RowVersion;
            _context.Tarif.Remove(tarif);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }
            
            return NoContent();
        }
    }
}