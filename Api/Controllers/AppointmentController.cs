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
using Appointment = UrbanSisters.Model.Appointment;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppointmentController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public AppointmentController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        
        // GET: /appointment
        [HttpGet]
        [ProducesResponseType(typeof(Dto.Page<Dto.Appointment>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(int? pageIndex = 0, int? pageSize = 5, bool? pro = false)
        {
            if (pageIndex.Value < 0 || pageSize.Value < 0)
            {
                return BadRequest();
            }
            
            IEnumerable<Appointment> appointments = await _context.Appointment.Where(appointment => appointment.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).Include(appointment => appointment.Relookeuse).Include(appointment => appointment.Relookeuse.User).OrderByDescending(appointment => appointment.Date).Skip(pageIndex.Value* pageSize.Value).Take(pageSize.Value).ToArrayAsync();
            
            int countTotalAppointment = await _context.Appointment.CountAsync();

            return Ok(new Dto.Page<Dto.Appointment>{Items = appointments.Select(appointment => _mapper.Map<Appointment, Dto.Appointment>(appointment)), PageIndex = pageIndex.Value, PageSize = pageSize.Value, TotalCount = countTotalAppointment});
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Get([FromBody] Dto.AppointmentRequest appointmentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (await _context.Appointment.CountAsync(ap => (!ap.Accepted || !ap.Finished) && ap.RelookeuseId == appointmentRequest.RelookeuseId && ap.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) > 0)
            {
                return Conflict();
            }
            
            Relookeuse relookeuse = await _context.Relookeuse.Include(rel => rel.User).FirstOrDefaultAsync(rel => rel.UserId == appointmentRequest.RelookeuseId);

            if (relookeuse == null)
            {
                return NotFound();
            }

            Appointment appointment = new Appointment
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                RelookeuseId = appointmentRequest.RelookeuseId,
                Date = DateTime.Now,
                Accepted = false,
                Makeup = appointmentRequest.MakeUp,
                Finished = false
            };

            var result = await _context.AddAsync(appointment);
            await _context.SaveChangesAsync();

            Dto.Appointment dtoAppointment = _mapper.Map<Appointment, Dto.Appointment>(result.Entity);
            dtoAppointment.RelookeuseFirstName = relookeuse.User.FirstName;
            dtoAppointment.RelookeuseLastName = relookeuse.User.LastName;
            
            return Created("api/appointment/"+result.Entity.Id, dtoAppointment);
        }
        
        [HttpPatch("{id}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Get(int id, [FromBody] Dto.Rating ratingRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Appointment appointment = await _context.Appointment.FirstOrDefaultAsync(ap => ap.Id == id && ap.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            if (appointment == null)
            {
                return NotFound();
            }

            if (!appointment.Accepted)
            {
                return BadRequest();
            }

            if (appointment.Finished)
            {
                return Conflict(ConflictErrorType.AppointmentAlreadyClose);
            }

            appointment.Finished = true;
            appointment.Mark = ratingRequest.Value;
            _context.Entry(appointment).OriginalValues["RowVersion"] = ratingRequest.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ConflictErrorType.AppointmentNewlyModified);
            }

            return NoContent();
        }
    }
}