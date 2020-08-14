using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class ImageServiceModel : ImageBase
    {
        [Required]
        public bool IsConvert { get; set; }        
        internal string FilePath { get; set; }
        public string FileExtension { get; set; }
        public virtual IEnumerable<ConvertedImageServiceModel> ConvertedImages { get; set; }
    }
}
