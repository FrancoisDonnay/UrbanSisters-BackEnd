using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    public class UserController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;

        public UserController(UrbanSisterContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        // GET: /user
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Dto.User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get()
        {
            User[] list = await _context.User.Include(user => user.Relookeuse).ToArrayAsync();
            
            return Ok(list.Select(user => _mapper.Map<Dto.User>(user)));
        }
    }
}
