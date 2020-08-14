using System.IO;

namespace ImageOptimizer.Models
{
    public class ConvertedImageServiceModel : ImageBase
    {
        internal string FilePath { get; set; }
        public string FileExtension { get; set; }
        public string Image { get; set; }

        public ConvertedImageServiceModel(string convertedImagePath, ImageServiceModel imageServiceModel)
        {
            FilePath = convertedImagePath;
            Name = Path.GetFileName(convertedImagePath);
            FileExtension = Path.GetExtension(convertedImagePath);
            OriginalSize = imageServiceModel.OptimizedSize;
            OptimizedSize = new FileInfo(convertedImagePath).Length;
            IsLossless = imageServiceModel.IsLossless;
            OptimizationLevel = imageServiceModel.OptimizationLevel;
        }
    }
}
