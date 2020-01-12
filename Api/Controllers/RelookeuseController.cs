using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RelookeuseController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly JwtIssuerOptions _jwtOptions;

        public RelookeuseController(UrbanSisterContext context, IMapper mapper, BlobServiceClient blobServiceClient, IOptions<JwtIssuerOptions> jwtOptions)
        {
            this._context = context;
            this._mapper = mapper;
            this._blobContainerClient = blobServiceClient.GetBlobContainerClient("profilepicture");
            this._jwtOptions = jwtOptions.Value;
        }
        
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Dto.Page<Dto.Relookeuse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(int? pageIndex = 0, int? pageSize = 5)
        {
            if (pageIndex.Value < 0 || pageSize.Value < 0)
            {
                return BadRequest();
            }
            
            IEnumerable<Relookeuse> relookeuseAtPage = await _context.Relookeuse.Include(relookeuse => relookeuse.User).Include(relookeuse => relookeuse.Appointment).OrderByDescending(relookeuse => relookeuse.Appointment.Sum(appointment => appointment.Mark)/relookeuse.Appointment.Count(appointment => appointment.Mark != null)).Skip(pageIndex.Value* pageSize.Value).Take(pageSize.Value).ToArrayAsync();
            int countTotalRelookeuse = await _context.Relookeuse.CountAsync();

            return Ok(new Dto.Page<Dto.Relookeuse>{Items = relookeuseAtPage.Select(relookeuse => _mapper.Map<Relookeuse, Dto.Relookeuse>(relookeuse)), PageIndex = pageIndex.Value, PageSize = pageSize.Value, TotalCount = countTotalRelookeuse});
        }
        
        // GET: /relookeuse
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Dto.DetailedRelookeuse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            Relookeuse relookeuse = await _context.Relookeuse.Where(rel=> rel.UserId == id).Include(rel => rel.User).Include(rel => rel.Appointment).Include(rel => rel.PortfolioPicture).Include(rel => rel.Tarif).Include(rel => rel.Availability).FirstOrDefaultAsync();
            
            if (relookeuse == null)
            {
                return NotFound(id);
            }

            relookeuse.Availability = relookeuse.Availability.OrderBy(availability => availability.StartTime).ToList();
            
            return Ok(_mapper.Map<Relookeuse, Dto.DetailedRelookeuse>(relookeuse));
        }
        
        // GET: /relookeuse/me
        [HttpGet("me")]
        [Authorize(Roles = "relookeuse")]
        [ProducesResponseType(typeof(Dto.Relookeuse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyDetails()
        {
            Relookeuse relookeuse = await _context.Relookeuse.Where(rel => rel.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).Include(relookeuse => relookeuse.User).Include(relookeuse => relookeuse.Appointment).OrderByDescending(relookeuse => relookeuse.Appointment.Sum(appointment => appointment.Mark)/relookeuse.Appointment.Count(appointment => appointment.Mark != null)).FirstOrDefaultAsync();
            
            if (relookeuse == null)
            {
                return NotFound(int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            }
            
            return Ok(_mapper.Map<Relookeuse, Dto.Relookeuse>(relookeuse));
        }
        
        [HttpPatch("picture")]
        [Authorize(Roles = "relookeuse")]
        [ProducesResponseType(typeof(Dto.ProfilPicture), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReplacePicture([FromForm]Dto.Picture picture)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Relookeuse relookeuse = await _context.Relookeuse.FirstOrDefaultAsync(r => r.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            string oldPicture = relookeuse.Picture;
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(picture.File.FileName));
            
            relookeuse.Picture = blobClient.Uri.ToString();
            
            _context.Entry(relookeuse).OriginalValues["RowVersion"] = picture.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
                
                if (oldPicture != null)
                {
                    await _blobContainerClient.GetBlobClient(oldPicture.Split("/").Last()).DeleteIfExistsAsync();
                }
                await using var stream = picture.File.OpenReadStream();
                await blobClient.UploadAsync(stream);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ConflictErrorType.RelookeuseNewlyModified);
            }
            
            return Ok(new Dto.ProfilPicture{Url = blobClient.Uri.ToString(), RowVersion = relookeuse.RowVersion});
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Dto.NewRelookeuse), StatusCodes.Status201Created)]
        public async Task<IActionResult> NewInscription([FromBody] Dto.RelookeuseInscription relookeuseInscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            if(_context.Relookeuse.FirstOrDefault(rel => rel.UserId == userId) != null)
            {
                return Conflict();
            }

            Relookeuse relookeuse = _mapper.Map<Relookeuse>(relookeuseInscription);
            relookeuse.UserId = userId;

            await _context.AddAsync(relookeuse);
            await _context.SaveChangesAsync();

            relookeuse = await _context.Relookeuse.Include(rel => rel.User).FirstOrDefaultAsync(rel => rel.UserId == userId);

            Dto.NewRelookeuse newRelookeuse = _mapper.Map<Dto.NewRelookeuse>(relookeuse);
            newRelookeuse.NewToken = await Utils.CreateTokenFor(relookeuse.User, _jwtOptions);
            
            return Created("api/relookeuse/" + userId, newRelookeuse);
        }
        
        [HttpDelete("picture")]
        [Authorize(Roles = "relookeuse")]
        [ProducesResponseType(typeof(Dto.RelookeuseRowVersion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeletePicture([FromBody] Dto.RelookeuseRowVersion relRowVersion)
        {
            Relookeuse relookeuse = await _context.Relookeuse.FirstOrDefaultAsync(r => r.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            string oldPicture = relookeuse.Picture;
            
            relookeuse.Picture = null;
            
            _context.Entry(relookeuse).OriginalValues["RowVersion"] = relRowVersion.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
                
                if (oldPicture != null)
                {
                    await _blobContainerClient.GetBlobClient(oldPicture.Split("/").Last()).DeleteIfExistsAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(ConflictErrorType.RelookeuseNewlyModified);
            }
            
            return Ok(new Dto.RelookeuseRowVersion{RowVersion = relookeuse.RowVersion});
        }
    }
}