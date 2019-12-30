﻿using System;
using System.Collections.Generic;
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
        [AllowAnonymous]
        [ProducesResponseType(typeof(Dto.Page<Dto.Relookeuse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(int? pageIndex = 0, int? pageSize = 5)
        {
            if (pageIndex.Value < 0 || pageSize.Value < 0)
            {
                return BadRequest();
            }
            
            IEnumerable<Relookeuse> relookeuseAtPage = await _context.Relookeuse.Include(relookeuse => relookeuse.User).Include(relookeuse => relookeuse.Appointment).OrderByDescending((relookeuse => (relookeuse.Appointment.Sum(appointment => appointment.Mark)/relookeuse.Appointment.Count(appointment => appointment.Mark != null)))).Skip(pageIndex.Value* pageSize.Value).Take(pageSize.Value).ToArrayAsync();
            int countTotalRelookeuse = await _context.Relookeuse.CountAsync();

            return Ok(new Dto.Page<Dto.Relookeuse>{Items = relookeuseAtPage.Select(relookeuse => _mapper.Map<Relookeuse, Dto.Relookeuse>(relookeuse)), PageIndex = pageIndex.Value, PageSize = pageSize.Value, TotalCount = countTotalRelookeuse});
        }
        
        // GET: /relookeuse
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(int id)
        {
            if (id < 1)
            {
                return BadRequest();
            }

            Relookeuse relookeuse = await _context.Relookeuse.Where(relookeuse => relookeuse.UserId == id).Include(relookeuse => relookeuse.User).Include(relookeuse => relookeuse.Appointment).Include(relookeuse => relookeuse.PortfolioPicture).Include(relookeuse => relookeuse.Tarif).Include(relookeuse => relookeuse.Availability).FirstOrDefaultAsync();
            
            if (relookeuse == null)
            {
                return NotFound(id);
            }

            relookeuse.Availability = relookeuse.Availability.OrderBy(availability => availability.StartTime).ToList();
            
            return Ok(_mapper.Map<Relookeuse, Dto.DetailedRelookeuse>(relookeuse));
        }
        
        [HttpPost("picture/{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadPicture(int id, [FromForm]Dto.Picture picture)
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
        public async Task<IActionResult> ReplacePicture(int id, [FromForm]Dto.Picture picture)
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