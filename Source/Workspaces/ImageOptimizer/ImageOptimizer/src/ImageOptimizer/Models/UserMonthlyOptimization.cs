using System;
using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class UserMonthlyOptimization
    {
        public int Id { get; set; }

        public int MonthlyOptimizedImages { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
