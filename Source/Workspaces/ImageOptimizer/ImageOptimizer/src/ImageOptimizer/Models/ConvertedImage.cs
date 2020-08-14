namespace ImageOptimizer.Models
{
    public class ConvertedImage : ImageBase
    {
        public int Id { get; set; }

        public int OriginalImageId { get; set; }
        public virtual Image OriginalImage { get; set; }
        
        public ConvertedImage(ConvertedImageServiceModel convertedImageServiceModel)
        {
            Name = convertedImageServiceModel.Name;
            FileType = convertedImageServiceModel.FileType;
            OriginalSize = convertedImageServiceModel.OriginalSize;
            OptimizedSize = convertedImageServiceModel.OptimizedSize;
            IsLossless = convertedImageServiceModel.IsLossless;
            OptimizationLevel = convertedImageServiceModel.OptimizationLevel;
            OptimizationTime = convertedImageServiceModel.OptimizationTime;
        }
    }
}
