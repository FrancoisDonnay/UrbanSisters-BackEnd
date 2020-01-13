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
            User user = await _context.User.Include(u => u.Relookeuse).Where(u => u.Id == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized();
            }
            
            return Ok(_mapper.Map<Dto.User>(user));
        }
        
        [HttpGet]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(Dto.Page<Dto.User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(int? pageIndex = 0, int? pageSize = 5)
        {
            if (pageIndex.Value < 0 || pageSize.Value < 0)
            {
                return BadRequest();
            }
            
            IEnumerable<User> usersAtPage = await _context.User.Include(u => u.Relookeuse).Skip(pageIndex.Value* pageSize.Value).Take(pageSize.Value).ToArrayAsync();
            int countTotalUsers = await _context.User.CountAsync();

            return Ok(new Dto.Page<Dto.User>{Items = usersAtPage.Select(u => _mapper.Map<User, Dto.User>(u)), PageIndex = pageIndex.Value, PageSize = pageSize.Value, TotalCount = countTotalUsers});
        }
        
        [HttpPatch("admin")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(Dto.UserRowVersion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SetAdmin([FromBody] Dto.SetAdminValue setAdminValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            User user = await _context.User.FirstOrDefaultAsync(u => u.Id == setAdminValue.Id);

            if (user == null)
            {
                return NotFound(setAdminValue.Id);
            }

            user.IsAdmin = setAdminValue.IsAdmin;
            
            _context.Entry(user).OriginalValues["RowVersion"] = setAdminValue.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return Ok(new Dto.UserRowVersion(){RowVersion = user.RowVersion});
        }
        
        //PATCH: /user
        [HttpPatch]
        [ProducesResponseType(typeof(Dto.User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PatchUser([FromBody] Dto.UserChange userChange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            User user = await _context.User.Where(u => u.Id == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).FirstOrDefaultAsync();
            
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
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ConflictErrorType.UserNewlyModified);
            }

            return Ok(_mapper.Map<Dto.User>(user));
        }
    }
}
