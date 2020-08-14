using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class ImageBase
    {
        [Display(Name = "Name")]
        [StringLength(255, MinimumLength = 5)]
        public string Name { get; set; }

        [StringLength(100, MinimumLength = 5)]
        public string FileType { get; set; }

        [Display(Name = "Original Size")]
        public long OriginalSize { get; set; }

        [Display(Name = "Optimized Size")]
        public long OptimizedSize { get; set; }

        [Display(Name = "Savings")]
        public long Savings => OriginalSize - OptimizedSize;

        [Display(Name = "Savings %")]
        public double SavingsInPercentage => (double)Savings / OriginalSize;

        [Required]
        public bool IsLossless { get; set; }
        
        public int OptimizationLevel { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Uploaded { get; } = DateTime.UtcNow;

        [DataType(DataType.Time)]
        public TimeSpan OptimizationTime { get; set; }
    }
}
