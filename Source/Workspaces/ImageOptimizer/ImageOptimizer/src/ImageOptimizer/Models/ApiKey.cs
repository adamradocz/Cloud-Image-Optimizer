using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        
        [Display(Name = "Api Key")]
        [StringLength(100, MinimumLength = 32)]
        public string Key { get; set; }

        [Display(Name = "Label")]
        [StringLength(512)]
        public string Label { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
