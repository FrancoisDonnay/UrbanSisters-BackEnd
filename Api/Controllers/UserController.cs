using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public UserController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        
        // GET: /user/me
        [HttpGet("me")]
        [ProducesResponseType(typeof(Dto.User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            User user = await _context.User.Include(user => user.Relookeuse).Where(u => u.Id == Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized();
            }
            
            return Ok(_mapper.Map<Dto.User>(user));
        }

        // GET: /user
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Dto.User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Get()
        {
            User[] list = await _context.User.Include(user => user.Relookeuse).ToArrayAsync();
            
            return Ok(list.Select(user => _mapper.Map<Dto.User>(user)));
        }
        
        //PATCH: /user
        [HttpPatch]
        [ProducesResponseType(typeof(Dto.User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PatchUser([FromBody] Dto.UserChange userChange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            User user = await _context.User.Where(u => u.Id == Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
            
            if (user == null)
            {
                return Unauthorized();
            }

            if (await _context.User.Where(u => u.Email.Equals(userChange.Email.ToLower())).FirstOrDefaultAsync() != null)
            {
                return Conflict(ConflictErrorType.EmailAlreadyUsed);
            }

            user.Email = userChange.Email.ToLower();
            _context.Entry(user).OriginalValues["RowVersion"] = userChange.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Conflict(ConflictErrorType.UserNewlyModified);
            }

            return Ok(_mapper.Map<Dto.User>(user));
        }
    }
}
