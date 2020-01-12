using System;
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
    public class AvailabilityController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public AvailabilityController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Dto.Availability>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyAvailabilities()
        {
            IEnumerable<Availability> availabilities = await _context.Availability.Where(av => av.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).OrderBy(av=> av.StartTime).ToListAsync();
            return Ok(availabilities.Select(av => _mapper.Map<Dto.Availability>(av)));
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Dto.Availability), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddAvailability([FromBody] Dto.NewAvailability newAvailability)
        {
            if (!ModelState.IsValid || newAvailability.StartTime.Equals(newAvailability.EndTime) || String.CompareOrdinal(newAvailability.StartTime, 0, newAvailability.EndTime, 0, 5) > 0)
            {
                return BadRequest();
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (await _context.Availability.FirstOrDefaultAsync(av => av.RelookeuseId == userId && av.DayOfWeek == newAvailability.DayOfWeek && (av.StartTime.Equals(newAvailability.StartTime) || av.EndTime.Equals(newAvailability.EndTime)))!= null)
            {
                return Conflict();
            }
            
            Availability availability = _mapper.Map<Availability>(newAvailability);
            availability.RelookeuseId = userId;
            var result = await _context.AddAsync(availability);
            await _context.SaveChangesAsync();
            
            return Created("api/availability/", _mapper.Map<Dto.Availability>(result.Entity));
        }
        
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Dto.Availability), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] Dto.Availability updatedAvailability)
        {
            if (!ModelState.IsValid || updatedAvailability.StartTime.Equals(updatedAvailability.EndTime) || String.CompareOrdinal(updatedAvailability.StartTime, 0, updatedAvailability.EndTime, 0, 5) > 0)
            {
                return BadRequest(ModelState);
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            Availability availability = await _context.Availability.FirstOrDefaultAsync(av => av.Id == updatedAvailability.Id && av.RelookeuseId == userId);

            if (availability == null)
            {
                return NotFound(updatedAvailability.Id);
            }
            
            if (await _context.Availability.FirstOrDefaultAsync(av => av.RelookeuseId == userId && av.Id != updatedAvailability.Id && av.DayOfWeek == updatedAvailability.DayOfWeek && (av.StartTime.Equals(updatedAvailability.StartTime) || av.EndTime.Equals(updatedAvailability.EndTime)))!= null)
            {
                return Conflict(ConflictErrorType.AvailabilityAlreadyExist);
            }

            availability.StartTime = updatedAvailability.StartTime;
            availability.EndTime = updatedAvailability.EndTime;

            _context.Entry(availability).OriginalValues["RowVersion"] = updatedAvailability.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ConflictErrorType.AvailabilityNewlyModified);
            }
            
            return Ok(_mapper.Map<Dto.Availability>(availability));
        }
        
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete([FromBody] Dto.DeleteAvailability delAvailability)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            Availability availability = await _context.Availability.FirstOrDefaultAsync(av => av.Id == delAvailability.Id  && av.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            
            if (availability == null)
            {
                return NotFound(delAvailability.Id);
            }
            
            _context.Entry(availability).OriginalValues["RowVersion"] = delAvailability.RowVersion;
            _context.Availability.Remove(availability);

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