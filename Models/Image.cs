using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required byte[] ImageData { get; set; }
        
        [Required]
        public required string ContentType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
    }
} 