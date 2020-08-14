namespace ImageOptimizer.Models
{
    public class RolePermissionBase
    {
        public string AllowedFileTypes { get; set; }
        public string AllowedFileExtensions { get; set; }
        public int AllowedImageSize { get; set; }
        public int ImageLimitPerMonth { get; set; }
        public bool CanOptimizeLossy { get; set; }
        public bool CanConvertToWebp { get; set; }
        public bool CanConvertToJxr { get; set; }
        public bool CanConvertToApng { get; set; }
        public bool CanConvertToJpeg2000 { get; set; }
        public int OptimizationLevel { get; set; }
        public bool CanUseMultisite { get; set; }
    }
}
