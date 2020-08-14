using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class Country
    {
        public int Id { get; set; }

        public string EnglishName { get; set; }

        [StringLength(2)]
        public string Alpha2Code { get; set; }

        [StringLength(3)]
        public string Alpha3Code { get; set; }

        public int NumericCode { get; set; }
    }
}
