using System.ComponentModel.DataAnnotations;

namespace ImageOptimizer.Models
{
    public class AddressBase
    {
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string StreetAddress { get; set; }
        
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Zip code")]
        public string ZipCode { get; set; }

        [Required]
        [Display(Name = "Country")]
        public int CountryId { get; set; }
    }
}
