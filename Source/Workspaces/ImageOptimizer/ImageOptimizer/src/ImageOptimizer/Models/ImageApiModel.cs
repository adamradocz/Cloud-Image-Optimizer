using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class ImageApiModel : ImageServiceModel
    {
        [Required]
        public string ApiKey { get; set; }
        public string Image { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
    }
}
