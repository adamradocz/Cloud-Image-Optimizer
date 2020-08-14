namespace ImageOptimizer.Models.PermissionViewModels
{
    public class GetPermissionsByApiKeyViewModel : RolePermissionBase
    {
        public GetPermissionsByApiKeyViewModel(RolePermission rolePermission)
        {
            AllowedFileTypes = rolePermission.AllowedFileTypes;
            AllowedFileExtensions = rolePermission.AllowedFileExtensions;
            AllowedImageSize = rolePermission.AllowedImageSize;
            ImageLimitPerMonth = rolePermission.ImageLimitPerMonth;
            CanOptimizeLossy = rolePermission.CanOptimizeLossy;
            CanConvertToWebp = rolePermission.CanConvertToWebp;
            CanConvertToJxr = rolePermission.CanConvertToJxr;
            CanConvertToApng = rolePermission.CanConvertToApng;
            CanConvertToJpeg2000 = rolePermission.CanConvertToJpeg2000;
            OptimizationLevel = rolePermission.OptimizationLevel;
        }

        public string RoleName { get; set; }
        public string Message { get; set; }
    }
}
