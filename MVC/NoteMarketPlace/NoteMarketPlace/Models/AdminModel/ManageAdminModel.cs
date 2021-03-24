using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models.AdminModel
{
    public class ManageAdminModel
    {

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public decimal Phone { get; set; }

        public DateTime DateAdded { get; set; }

        public string Active { get; set; }


    }
}