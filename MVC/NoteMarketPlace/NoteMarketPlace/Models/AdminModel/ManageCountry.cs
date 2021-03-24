using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models.AdminModel
{
    public class ManageCountry
    {

        public int Id { get; set; }

        public string CountryName { get; set; }

        public decimal CountryCode { get; set; }

        public string AddedBy { get; set; }

        public DateTime AddedDate { get; set; }

        public string Active { get; set; }

    }
}