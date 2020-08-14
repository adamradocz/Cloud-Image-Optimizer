using System.Collections.Generic;
using System.Linq;

namespace ImageOptimizer.Models
{
    public class Image : ImageBase
    {
        public int Id { get; set; }
        public ICollection<ConvertedImage> ConvertedImages { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public Image(string applicationUserId, ImageServiceModel imageServiceModel)
        {
            ApplicationUserId = applicationUserId;
            Name = imageServiceModel.Name;
            FileType = imageServiceModel.FileType;
            OriginalSize = imageServiceModel.OriginalSize;
            OptimizedSize = imageServiceModel.OptimizedSize;
            IsLossless = imageServiceModel.IsLossless;
            OptimizationLevel = imageServiceModel.OptimizationLevel;
            OptimizationTime = imageServiceModel.OptimizationTime;

            if (imageServiceModel.ConvertedImages != null)
            {
                ConvertedImages = imageServiceModel.ConvertedImages
                    .Select(convertedImageServiceModel => new ConvertedImage(convertedImageServiceModel))
                    .ToList();
            }
        }
    }
}
