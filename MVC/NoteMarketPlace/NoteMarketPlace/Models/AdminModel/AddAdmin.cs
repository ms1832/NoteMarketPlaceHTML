using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoteMarketPlace.Models.AdminModel
{
    public class AddAdmin
    {

        public int Id { get; set; }

        [Required(ErrorMessage ="* First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "* Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "* Email address is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "required")]
        public string CountryCode { get; set; }

        [Required(ErrorMessage = "* Phone is required")]
        public decimal Phone { get; set; }


        public List<CountryModel> CountryModel { get; set; }



        public void MaptoModel(User user, User_Details details)
        {
            user.First_Name = FirstName;
            user.Last_Name = LastName;
            user.Email = Email;
            details.Phone_No_Country_Code = CountryCode;
            details.Phone_No = Phone;
        }


    }

}