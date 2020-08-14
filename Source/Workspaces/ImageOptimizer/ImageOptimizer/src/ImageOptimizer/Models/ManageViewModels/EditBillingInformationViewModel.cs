using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ImageOptimizer.Models.ManageViewModels
{
    public class EditBillingInformationViewModel : AddressBase, IEquatable<Address>
    {
        public IEnumerable<SelectListItem> Countries { get; set; }
        
        public bool Equals(Address address)
        {
            return City == address.City &&
                   CompanyName == address.CompanyName &&
                   CountryId == address.CountryId &&
                   FirstName == address.FirstName &&
                   LastName == address.LastName &&
                   State == address.State &&
                   StreetAddress == address.StreetAddress &&
                   ZipCode == address.ZipCode;
        }
    }
}
