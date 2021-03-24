using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteMarketPlace.Models.AdminModel
{
    public class RejectedNotesModel
    {

        public int NoteId { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public int SellerId { get; set; }

        public string SellerName { get; set; }

        public DateTime DateEdited { get; set; }

        public string RejectedBy { get; set; }

        public string Remarks { get; set; }



    }
}