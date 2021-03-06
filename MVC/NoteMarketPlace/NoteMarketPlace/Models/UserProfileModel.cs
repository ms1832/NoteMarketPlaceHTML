using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models
{
    public class UserProfileModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public Nullable<DateTime> DOB { get; set; }

        public Nullable<int> Gender { get; set; }

        [Required]
        public string CountryCode { get; set; }

        [Required]
        public decimal Phone { get; set; }

        public string ProfilePicture { get; set; }

        [Required]
        public string Address1 { get; set; }

        [Required]
        public string Address2 { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Zipcode { get; set; }

        public int Country { get; set; }
        
        [Required]
        public string University { get; set; }

        [Required]
        public string College { get; set; }


        public List<GenderModel> genderModel { get; set; }

        public List<CountryModel> countryModel { get; set; }

        public List<CountryModel> CountryCodeModel { get; set; }


        public void MaptoModel(User user, User_Details details)
        {
            user.First_Name = FirstName;
            user.Last_Name = LastName;
            user.Email = Email;
            details.DOB = DOB;
            details.Gender = Gender;
            details.Phone_No_Country_Code = CountryCode;
            details.Phone_No = Phone;
            details.Profile_Img = ProfilePicture;
            details.Address_Line1 = Address1;
            details.Address_Line2 = Address2;
            details.City = City;
            details.State = State;
            details.ZipCode = Zipcode;
            details.Country_Id = Country;
            details.College = College;
            details.University = University;
        }

    }


    public class GenderModel
    {
        public int Gender_Id { get; set; }
        public string Gender_Val { get; set; }
    }

    public class CountryModel
    {
        public int Country_Id { get; set; }
        public string Country_Val { get; set; }
        public decimal Country_Code { get; set; }
    }


}