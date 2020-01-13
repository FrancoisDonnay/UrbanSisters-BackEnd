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
using UrbanSisters.Dal;
using UrbanSisters.Model;

namespace UrbanSisters.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "relookeuse")]
    public class PortfolioController : ControllerBase
    {
        private readonly UrbanSisterContext _context;
        private readonly IMapper _mapper;
        private readonly BlobContainerClient _blobContainerClient;

        public PortfolioController(UrbanSisterContext context, IMapper mapper, BlobServiceClient blobServiceClient)
        {
            this._context = context;
            this._mapper = mapper;
            this._blobContainerClient = blobServiceClient.GetBlobContainerClient("portfoliopicture");
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Dto.PortfolioPicture>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyAvailabilities()
        {
            IEnumerable<PortfolioPicture> portfolioPictures = await _context.PortfolioPicture.Where(pp => pp.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)).ToListAsync();
            return Ok(portfolioPictures.Select(pp => _mapper.Map<Dto.PortfolioPicture>(pp)));
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Dto.PortfolioPicture), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddPicture([FromForm]Dto.NewPortfolioPicture picture)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            BlobClient blobClient = _blobContainerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(picture.File.FileName));
            
            PortfolioPicture portfolioPicture = new PortfolioPicture();
            portfolioPicture.Picture = blobClient.Uri.ToString();
            portfolioPicture.RelookeuseId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _context.AddAsync(portfolioPicture);
            await using var stream = picture.File.OpenReadStream();
            
            
            Task uploadImageTask = blobClient.UploadAsync(stream);
            Task saveDatabase = _context.SaveChangesAsync();

            await Task.WhenAll(uploadImageTask, saveDatabase);

            return Created("api/portfolio/",_mapper.Map<Dto.PortfolioPicture>(result.Entity));
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Dto.ApiError), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeletePicture(int id)
        {
            PortfolioPicture portfolioPicture = await _context.PortfolioPicture.FirstOrDefaultAsync(pp => pp.Id == id && pp.RelookeuseId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            if (portfolioPicture == null)
            {
                return NotFound(id);
            }
            
            Task removeImageTask = _blobContainerClient.GetBlobClient(portfolioPicture.Picture.Split("/").Last()).DeleteIfExistsAsync();
            _context.Remove(portfolioPicture);
            Task removeInDbTasj = _context.SaveChangesAsync();
            await Task.WhenAll(removeImageTask, removeInDbTasj);
            return NoContent();
        }
    }
}