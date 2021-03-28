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

        [Required(ErrorMessage ="* Required Field")]
        public string First_Name { get; set; }

        [Required(ErrorMessage = "* Required Field")]
        public string Last_Name { get; set; }

        public string Email { get; set; }

        public string SecondaryEmail { get; set; }

        public string Phonecode { get; set; }

        public decimal Phone { get; set; }

        public string ProfileImage { get; set; }

        public HttpPostedFileBase Image { get; set; }




        public List<CountryModel> PhoneCodeModel { get; set; }



        public void MAptoModel(User user, User_Details details)
        {
            user.First_Name = First_Name;
            user.Last_Name = Last_Name;
            details.Secondary_Email = SecondaryEmail;
            //details.Profile_Img = ProfileImage;
            details.Phone_No = Phone;
            details.Phone_No_Country_Code = Phonecode;
        }

    }

}