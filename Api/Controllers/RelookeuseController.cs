using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanSisters.Dal;
using UrbanSisters.Dto;
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

        public RelookeuseController(UrbanSisterContext context, IMapper mapper, BlobContainerClient blobContainerClient)
        {
            this._context = context;
            this._mapper = mapper;
            this._blobContainerClient = blobContainerClient;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Relookeuse.ToArrayAsync());
        }
        
        // GET: /relookeuse
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(int id)
        {
            Relookeuse relookeuse = await _context.Relookeuse.FirstOrDefaultAsync(user => user.UserId == id);

            if (relookeuse == null)
            {
                return NotFound(id);
            }

            return Ok(relookeuse);
        }

        [HttpPost("picture/{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadPicture(int id, [FromForm]Picture picture)
        {
            if (id < 1)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Relookeuse relookeuse = await _context.Relookeuse.FirstOrDefaultAsync(r => r.UserId == id);
            
            if (relookeuse.Picture != null)
            {
                return BadRequest();
            }
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid()+ Path.GetExtension(picture.File.FileName));
            
            await using var stream = picture.File.OpenReadStream();
            Task saveInCloudTask = blobClient.UploadAsync(stream);
            
            relookeuse.Picture = blobClient.Uri.ToString();
            _context.Update(relookeuse);
            Task saveDatabaseTask = _context.SaveChangesAsync();

            await Task.WhenAll(saveInCloudTask, saveDatabaseTask);
            
            return Created(blobClient.Uri, new {userId = relookeuse.UserId, pictureUrl = blobClient.Uri});
        }
        
        [HttpPatch("picture/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReplacePicture(int id, [FromForm]Picture picture)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Relookeuse relookeuse = await _context.Relookeuse.FirstOrDefaultAsync(r => r.UserId == id);
            
            if (relookeuse.Picture == null)
            {
                return BadRequest();
            }
            
            Task removeOldBlobTask = _blobContainerClient.GetBlobClient(relookeuse.Picture.Split("/").Last()).DeleteIfExistsAsync();
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(picture.File.FileName));;
            
            await using var stream = picture.File.OpenReadStream();
            Task saveInCloudTask = blobClient.UploadAsync(stream);

            relookeuse.Picture = blobClient.Uri.ToString();
            _context.Update(relookeuse);
            Task saveDatabaseTask = _context.SaveChangesAsync();
            
            await Task.WhenAll(removeOldBlobTask, saveInCloudTask, saveDatabaseTask);
            
            return Ok(new {userId = relookeuse.UserId, pictureUrl = blobClient.Uri});
        }
    }
}