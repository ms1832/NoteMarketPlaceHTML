using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class MyProfile
    {

        public int ID { get; set; }

        [Required]
        public string First_Name { get; set; }

        [Required]
        public string Last_Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string SecondaryEmail { get; set; }

        public string Phonecode { get; set; }

        public decimal Phone { get; set; }

        [Required]
        public string ProfileImage { get; set; }


        public List<CountryModel> PhoneCodeModel { get; set; }



    }

}