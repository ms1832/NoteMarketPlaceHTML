using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class AddCountryModel
    {

        public int Id { get; set; }

        [Required]
        public string CountryName { get; set; }

        [Required]
        public decimal CountryCode { get; set; }


        public void MaptoModel(Country_Details country)
        {
            country.Country_Name = CountryName;
            country.Country_Code = CountryCode;
        }


    }
}