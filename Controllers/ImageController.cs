using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using System.Net.Mime;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageController> _logger;

        public ImageController(ApplicationDbContext context, ILogger<ImageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with no file");
                    return BadRequest("No file uploaded");
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var image = new Image
                {
                    Name = file.FileName,
                    ImageData = memoryStream.ToArray(),
                    ContentType = file.ContentType
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Image uploaded successfully. ID: {Id}, Name: {Name}", image.Id, image.Name);
                return Ok(new { id = image.Id, name = image.Name, message = "Image uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, "Error uploading image");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            try
            {
                var image = await _context.Images.FindAsync(id);
                if (image == null)
                {
                    _logger.LogWarning("Image not found with ID: {Id}", id);
                    return NotFound($"Image with ID {id} not found");
                }

                _logger.LogInformation("Image retrieved successfully. ID: {Id}, Name: {Name}", image.Id, image.Name);
                return File(image.ImageData, image.ContentType ?? "application/octet-stream", image.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image with ID: {Id}", id);
                return StatusCode(500, "Error retrieving image");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, IFormFile file)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            image.Name = file.FileName;
            image.ImageData = memoryStream.ToArray();
            image.ContentType = file.ContentType;
            image.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { id = image.Id, name = image.Name });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var image = await _context.Images.FindAsync(id);
                if (image == null)
                {
                    _logger.LogWarning("Delete attempted for non-existent image. ID: {Id}", id);
                    return NotFound($"Image with ID {id} not found");
                }

                _context.Images.Remove(image);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Image deleted successfully. ID: {Id}, Name: {Name}", id, image.Name);
                return Ok(new { message = $"Image {image.Name} deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image with ID: {Id}", id);
                return StatusCode(500, "Error deleting image");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            try
            {
                var images = await _context.Images
                    .Select(i => new { 
                        i.Id, 
                        i.Name, 
                        i.CreatedAt, 
                        i.ModifiedAt,
                        thumbnailUrl = $"/api/image/{i.Id}" 
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} images from database", images.Count);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all images");
                return StatusCode(500, "Error retrieving images");
            }
        }
    }
} 